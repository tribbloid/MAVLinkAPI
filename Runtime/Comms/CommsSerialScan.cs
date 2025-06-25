using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace MAVLinkAPI.Comms
{
    public class CommsSerialScan
    {
        public static bool foundport = false;
        public static ConcurrentBag<ICommsSerial> portinterface;

        private static object runlock = new();
        public static int run = 0;
        public static int running = 0;
        private static bool connect = false;

        private static int[] bauds = new int[] { 0, 115200, 57600, 38400, 19200, 9600 };

        public static void Scan(bool connect = false)
        {
            foundport = false;
            portinterface = new ConcurrentBag<ICommsSerial>();
            lock (runlock)
            {
                run = 1;
                running = 0;
            }

            CommsSerialScan.connect = connect;

            var portlist = SerialPort.GetPortNames();

            foreach (var portname in portlist) new Thread(o => { doread(portname.Clone()); }).Start();
        }

        private static void doread(object o)
        {
            lock (runlock)
            {
                running++;
            }

            var portname = (string)o;

            Console.WriteLine("Scanning {0}", portname);

            try
            {
                ICommsSerial port = new SerialPort();
                {
                    port.PortName = portname;

                    foreach (var baud in bauds)
                    {
                        // try default baud
                        if (baud != 0)
                            port.BaudRate = baud;

                        port.Open();
                        port.DiscardInBuffer();

                        // let data flow in
                        Thread.Sleep(2000);

                        lock (runlock)
                        {
                            if (run == 0)
                            {
                                port.Close();
                                return;
                            }
                        }

                        var available = port.BytesToRead;
                        var buffer = new byte[available];
                        var read = port.Read(buffer, 0, available);

                        Console.WriteLine("{0} {1}", portname, read);

                        if (read > 0)
                            using (var ms = new MemoryStream(buffer, 0, read))
                            {
                                var mav = new MAVLink.MavlinkParse();

                                try
                                {
                                    again:

                                    var packet = mav.ReadPacket(ms);

                                    if (packet != null && packet.Length > 0)
                                    {
                                        port.Close();

                                        Console.WriteLine("Found Mavlink on port {0} at {1}", port.PortName,
                                            port.BaudRate);

                                        foundport = true;
                                        portinterface.Add(port);

                                        if (connect)
                                        {
                                            lock (runlock)
                                            {
                                                running--;

                                                if (running == 0)
                                                    run = 0;
                                            }

                                            doConnect?.Invoke(port);
                                        }

                                        break;
                                    }

                                    Console.WriteLine(portname + " crc: " + mav.badCRC);
                                    goto again;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(portname + " " + ex.ToString());
                                }
                            }

                        try
                        {
                            port.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                        if (foundport)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(portname + " " + ex.ToString());
            }
            finally
            {
                lock (runlock)
                {
                    running--;

                    if (running == 0)
                        run = 0;
                }
            }

            Console.WriteLine("Scan port {0} Finished!!", portname);
        }

        public static event doconnect doConnect;

        public delegate void doconnect(ICommsSerial port);
    }
}