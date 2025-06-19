#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using MAVLinkAPI.Comms;
using MAVLinkAPI.Ext;
using MAVLinkAPI.Util;
using MAVLinkAPI.Util.NullSafety;
using MAVLinkAPI.Util.Resource;
using UnityEngine;

namespace MAVLinkAPI.Routing
{
    public enum Protocol
    {
        Tcp,
        Udp,
        Ws,
        UdpCl,
        Serial
    }

    public class IOStream : Cleanable
    {
        public static class Defaults
        {
            public static int baudRate = 57600;

            public static List<int> preferredBaudRates = new() { baudRate };
        }

        [Serializable]
        public struct ArgsT
        {
            public Protocol protocol;
            public string address; // IP address + port number
            public bool dtrEnabled;
            public bool rtsEnabled;

            public static ArgsT UDPLocalDefault = new()
            {
                // default QGroundControl MAVLink forwarding target
                protocol = Protocol.Udp,
                address = "localhost:14445",
            };

            public string URIString => $"{protocol}://{address}";

            public static ArgsT Parse(string s)
            {
                var parts = s.Split(new[] { "://" }, 2, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    throw new ArgumentException($"Invalid format for IOStream.ArgsT: {s}");
                }

                if (!Enum.TryParse<Protocol>(parts[0], true, out var protocol))
                {
                    throw new ArgumentException($"Invalid protocol: {parts[0]}");
                }

                return new ArgsT
                {
                    protocol = protocol,
                    address = parts[1]
                };
            }
        }

        public ArgsT Args;

        // TODO: generalised this to read from any () => Stream
        // private readonly ICommsSerial comm;


        public ICommsSerial MkComm()
        {
            lock (GlobalAccessLock)
            {
                var peerClosed = 0;

                // close others with same name
                var peers = this.Peers().ToList();
                Debug.LogWarning(
                    $"found {peers.Count()} Serial and {GlobalRegistry.GlobalCounter.Value} SafeClean objects");

                foreach (var peer in peers)
                    if (peer.Args.URIString == Args.URIString)
                    {
                        peer.Dispose();
                        peerClosed += 1;
                    }

                if (peerClosed > 0)
                {
                    Debug.LogWarning($"{peerClosed} peer(s) with name {Args.URIString} are disposed");
                    Thread.Sleep(1000);
                }
            }

            ICommsSerial MkCommRaw()
            {
                var parts = Args.address.Split(':');

                switch (Args.protocol)
                {
                    case Protocol.Tcp:
                        var tcp = new TcpSerial();
                        tcp.client = new TcpClient(parts[0], int.Parse(parts[1]));
                        tcp.autoReconnect = true;
                        return tcp;
                    case Protocol.Udp:
                        var udp = new UdpSerial();
                        udp.client = new UdpClient(parts[0], int.Parse(parts[1]));
                        return udp;

                    case Protocol.UdpCl:
                        var udpcl = new UdpSerialConnect();
                        udpcl.client = new UdpClient(parts[0], int.Parse(parts[1]));
                        return udpcl;

                    case Protocol.Ws:
                        var ws = new WebSocket();
                        ws.Port = Args.address;
                        ws.autoReconnect = true;
                        return ws;

                    case Protocol.Serial:
                        var serial = new SerialPort();
                        serial.PortName = Args.address;
                        return serial;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var result = MkCommRaw();
            result.DtrEnable = Args.dtrEnabled;
            result.RtsEnable = Args.rtsEnabled;

            result.BaudRate = Defaults.baudRate;
            return result;
        }

        private Maybe<ICommsSerial> _comm; // can only be initialised once, will be closed at the end of lifetime
        public ICommsSerial Comm => _comm.Lazy(() => MkComm());

        public TimeSpan MinReopenInterval = TimeSpan.FromSeconds(1);

        public Stream BaseStream => Comm.BaseStream;
        public int BytesToRead => Comm.BytesToRead;

        // locking to prevent multiple reads on serial port
        public readonly object ReadLock = new();
        public readonly object WriteLock = new();

        // getter and setter for baud rate
        public int BaudRate
        {
            get => Comm.BaudRate;
            set => Comm.BaudRate = value;
        }

        public IOStream(ArgsT args, Lifetime? lifetime = null) : base(lifetime)
        {
            Args = args;
        }


        protected override void DoClean()
        {
            // Close the serial port
            IsOpen = false;
            _comm.Select(v =>
            {
                v.Dispose();
                return v;
            });
        }

        // last time it was closed
        private DateTime _lastActiveTime = DateTime.MinValue;

        public bool IsOpen
        {
            get => Comm.IsOpen;
            set
            {
                lock (WriteLock)
                {
                    if (value != IsOpen)
                    {
                        Retry.UpTo(4).With(
                                TimeSpan.FromSeconds(0.5),
                                logException: true
                            )
                            .FixedInterval.Run((_, _) =>
                            {
                                if (value)
                                {
                                    // wait for a bit before opening the port
                                    // TODO: should be simplified
                                    var millisSinceClosed = (DateTime.Now - _lastActiveTime).TotalMilliseconds;

                                    if (millisSinceClosed < MinReopenInterval.TotalMilliseconds)
                                    {
                                        var waitMillis =
                                            (int)(MinReopenInterval.TotalMilliseconds - millisSinceClosed);
                                        Debug.Log($"Waiting {waitMillis} ms before opening port {Comm.PortName}");
                                        Thread.Sleep(waitMillis);
                                    }

                                    Comm.Open();
                                    Debug.Log(
                                        $"Connected to {Comm.PortName} at {Comm.BaudRate} baud ({Args})"
                                    );
                                }
                                else
                                {
                                    // from Unity_SerialPort
                                    try
                                    {
                                        // Close the serial port
                                        Comm.Close();
                                        _lastActiveTime = DateTime.Now;
                                    }
                                    catch (Exception ex)
                                    {
                                        if (Comm.IsOpen == false)
                                            // Failed to close the serial port. Uncomment if
                                            // you wish but this is triggered as the port is
                                            // already closed and or null.
                                            Debug.LogWarning(
                                                $"Error on closing but port already closed! {ex.GetMessageForDisplay()}");
                                        else
                                            throw;
                                    }
                                }

                                // assert
                                if (value != Comm.IsOpen)
                                    throw new IOException(
                                        $"Failed to set port {Comm.PortName} to {(value ? "open" : "closed")}, baud rate {Comm.BaudRate}");
                            });

                        Debug.Log(
                            $"Port {Comm.PortName} is now {(value ? "open" : "closed")}, baud rate {Comm.BaudRate}");
                    }
                }
            }
        }

        public void Disconnect()
        {
            IsOpen = false;
        }

        public void Connect(
            bool verifyWrite = true // TODO: how to verify read that is agnostic to message?
        )
        {
            IsOpen = true;

            if (verifyWrite)
            {
                var validateWriteData = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };

                WriteBytes(validateWriteData);
            }
        }


        public void WriteBytes(byte[] bytes)
        {
            lock (WriteLock)
            {
                Comm.Write(bytes, 0, bytes.Length);
            }
        }
    }
}