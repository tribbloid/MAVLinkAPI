#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using MAVLinkAPI.Comms;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;

namespace MAVLinkAPI.Routing
{
    public class IOStream : IDisposable
    {
        public enum Protocol
        {
            Tcp,
            Udp,
            Ws,
            UdpCl,
            Serial
        }


        public static string ProtocolPrompt => string.Join(", ", Enum.GetNames(typeof(Protocol)));

        public static class BaudRates
        {
            public static readonly int Default = 57600;

            public static List<int> preferred = new() { 38400, Default };

            public static List<int> all = new()
            {
                4800,
                9600,
                19200,
                38400,
                Default,
                115200
            };
        }

        [Serializable]
        public record ArgsT(
            Protocol Protocol,
            string Address, // IP address + port number
            bool DtrEnabled = false,
            bool RtsEnabled = false
        )

        {
            public static ArgsT UDPLocalDefault = new(
                // default QGroundControl MAVLink forwarding target
                Protocol.Udp,
                "localhost:14445"
            );

            public string URIString => $"{Protocol}://{Address}";

            public static ArgsT Parse(string s)
            {
                var parts = s.Split(new[] { "://" }, 2, StringSplitOptions.None);
                if (parts.Length != 2)
                    throw new ArgumentException($"Invalid format (must be <protocol>://<address>): {s}");

                if (!Enum.TryParse<Protocol>(parts[0], true, out var protocol))
                    throw new ArgumentException(
                        $"Invalid protocol (must be chosen from {ProtocolPrompt}): {parts[0]}");

                return new ArgsT(
                    protocol,
                    parts[1]
                );
            }
        }

        public readonly ArgsT Args;

        public IOStream(ArgsT args)
        {
            Args = args;
        }

        private Maybe<ICommsSerial> _comm; // can only be initialised once, will be closed at the end of lifetime

        // TODO: generalised this to read from any () => Stream
        public ICommsSerial Comm => _comm.Lazy(() => Comm_Mk());

        public ICommsSerial Comm_Mk()
        {
            ICommsSerial GetRawComm()
            {
                var parts = Args.Address.Split(':');

                switch (Args.Protocol)
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
                        ws.Port = Args.Address;
                        ws.autoReconnect = true;
                        return ws;

                    case Protocol.Serial:
                        var serial = new SerialPort();
                        serial.PortName = Args.Address;
                        return serial;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var result = GetRawComm();
            result.DtrEnable = Args.DtrEnabled;
            result.RtsEnable = Args.RtsEnabled;

            result.BaudRate = BaudRates.Default;
            return result;
        }

        public TimeSpan MinReopenInterval = TimeSpan.FromSeconds(1);

        public Stream BaseStream => Comm.BaseStream;

        public int BytesToRead => Comm.BytesToRead;

        public double Metric_BufferPressure
        {
            get
            {
                if (Comm.ReadBufferSize == 0) return 0;
                return (double)BytesToRead / Comm.ReadBufferSize;
            }
        }

        // locking to prevent multiple reads on serial port
        public readonly object ReadLock = new();
        public readonly object WriteLock = new();

        // getter and setter for baud rate
        public int BaudRate
        {
            get => Comm.BaudRate;
            set => Comm.BaudRate = value;
        }


        public void Dispose()
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
                        if (value)
                        {
                            // wait for a bit before opening the port
                            // TODO: should be simplified
                            var millisSinceClosed = (DateTime.Now - _lastActiveTime).TotalMilliseconds;

                            if (millisSinceClosed < MinReopenInterval.TotalMilliseconds)
                            {
                                var waitMillis =
                                    (int)(MinReopenInterval.TotalMilliseconds - millisSinceClosed);
                                // Debug.Log($"Waiting {waitMillis} ms before opening port {Comm.PortName}");
                                Thread.Sleep(waitMillis);
                            }

                            Comm.Open();
                            Debug.Log($"Connected to {Comm.PortName}, baud rate {Comm.BaudRate})");
                        }
                        else
                        {
                            // from Unity_SerialPort
                            try
                            {
                                Comm.Close();
                                _lastActiveTime = DateTime.Now;
                            }
                            catch (Exception ex)
                            {
                                if (Comm.IsOpen == false)
                                    // Failed to close the serial port. Uncomment if
                                    // you wish but this is triggered as the port is
                                    // already closed and or null.
                                    Debug.LogWarning($"Error on closing but port already closed! {ex.Message}");
                                else
                                    throw;
                            }
                        }

                        // assert
                        if (value != Comm.IsOpen)
                            throw new IOException(
                                $"Failed to set port {Comm.PortName} to {(value ? "open" : "closed")}, baud rate {Comm.BaudRate}");

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
            IsOpen = true;

            lock (WriteLock)
            {
                Comm.Write(bytes, 0, bytes.Length);
            }
        }
    }
}