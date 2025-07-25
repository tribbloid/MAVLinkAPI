using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MAVLinkAPI.log4net;

// dns, ip address
// tcplistner

namespace MAVLinkAPI.Comms
{
    public class CommsNTRIP : CommsBase, ICommsSerial, IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CommsNTRIP));

        private DateTime _lastnmea = DateTime.MinValue;
        public double alt = 0;
        public TcpClient client = new();
        private Stream st;

        private string host;

        public double lat = 0;
        public double lng = 0;
        public bool ntrip_v1 = false;
        private IPEndPoint RemoteIpEndPoint = new(IPAddress.Any, 0);
        private Uri remoteUri;

        private int retrys = 3;

        public CommsNTRIP()
        {
            ReadTimeout = 500;
        }

        public string Port { get; set; }

        public int WriteBufferSize { get; set; }
        public int WriteTimeout { get; set; }
        public bool RtsEnable { get; set; }

        public Stream BaseStream => st;

        public void toggleDTR()
        {
        }

        public int ReadTimeout
        {
            get; // { return client.ReceiveTimeout; }
            set; // { client.ReceiveTimeout = value; }
        }

        public int ReadBufferSize { get; set; }

        public int BaudRate { get; set; }

        public int DataBits { get; set; }

        public string PortName { get; set; }

        public int BytesToRead
        {
            get
            {
                /*Console.WriteLine(DateTime.Now.Millisecond + " tcp btr " + (client.Available + rbuffer.Length - rbufferread));*/
                SendNMEA();
                return client.Available;
            }
        }

        public int BytesToWrite => 0;

        public bool IsOpen
        {
            get
            {
                try
                {
                    return client.Client.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool DtrEnable { get; set; }

        public void Open()
        {
            if (client.Client.Connected)
            {
                log.Warn("ntrip socket already open");
                return;
            }

            log.Info("ntrip Open");

            var url = OnSettings("NTRIP_url", "");

            if (OnInputBoxShow("remote host", "Enter url (eg http://user:pass@host:port/mount)", ref url) ==
                inputboxreturn.Cancel)
                throw new Exception("Canceled by request");

            OnSettings("NTRIP_url", url, true);

            Open(url);
        }

        public static string PercentEncode(string value)
        {
            var retval = new StringBuilder();
            foreach (var c in value)
                if ((c >= 48 && c <= 57) || //0-9  
                    (c >= 65 && c <= 90) || //a-z  
                    (c >= 97 && c <= 122) || //A-Z                    
                    c == 45 || c == 46 || c == 95 || c == 126 || c == 64 || c == 47 ||
                    c == 58) // period, hyphen, underscore, tilde, @, :, /
                    retval.Append(c);
                else
                    retval.AppendFormat("%{0:X2}", (byte)c);
            return retval.ToString();
        }

        public void Open(string url)
        {
            // Need to ensure URI is % encoded, except the first "@", colons and backslashes
            var count = url.Split('@').Length - 1;

            if (count > 1)
            {
                var regex = new Regex("@");
                url = regex.Replace(url, "%40", 1);
            }

            url = PercentEncode(url);

            url = url.Replace("ntrip://", "http://");

            remoteUri = new Uri(url);

            doConnect();
        }

        public int Read(byte[] readto, int offset, int length)
        {
            VerifyConnected();

            SendNMEA();

            try
            {
                if (length < 1) return 0;

                return st.Read(readto, offset, length);
            }
            catch
            {
                throw new Exception("ntrip Socket Closed");
            }
        }

        public int ReadByte()
        {
            VerifyConnected();
            var count = 0;
            while (BytesToRead == 0)
            {
                Thread.Sleep(1);
                if (count > ReadTimeout)
                    throw new Exception("ntrip Timeout on read");
                count++;
            }

            var buffer = new byte[1];
            Read(buffer, 0, 1);
            return buffer[0];
        }

        public int ReadChar()
        {
            return ReadByte();
        }

        public string ReadExisting()
        {
            VerifyConnected();
            var data = new byte[client.Available];
            if (data.Length > 0)
                Read(data, 0, data.Length);

            var line = Encoding.ASCII.GetString(data, 0, data.Length);

            return line;
        }

        public void WriteLine(string line)
        {
            VerifyConnected();
            line = line + "\r\n";
            Write(line);
        }

        public void Write(string line)
        {
            VerifyConnected();
            var data = new ASCIIEncoding().GetBytes(line);
            Write(data, 0, data.Length);
        }

        public void Write(byte[] write, int offset, int length)
        {
            VerifyConnected();
            try
            {
                st.Write(write, offset, length);
            }
            catch
            {
            } //throw new Exception("Comport / Socket Closed"); }
        }

        public void DiscardInBuffer()
        {
            VerifyConnected();
            var size = client.Available;
            var crap = new byte[size];
            log.InfoFormat("ntrip DiscardInBuffer {0}", size);
            Read(crap, 0, size);
        }

        public string ReadLine()
        {
            var temp = new byte[4000];
            var count = 0;
            var timeout = 0;

            while (timeout <= 100)
            {
                if (!IsOpen) break;
                if (BytesToRead > 0)
                {
                    var letter = (byte)ReadByte();

                    temp[count] = letter;

                    if (letter == '\n') // normal line
                        break;

                    count++;
                    if (count == temp.Length)
                        break;
                    timeout = 0;
                }
                else
                {
                    timeout++;
                    Thread.Sleep(5);
                }
            }

            Array.Resize(ref temp, count + 1);

            return Encoding.ASCII.GetString(temp, 0, temp.Length);
        }

        public void Close()
        {
            try
            {
                if (client.Client != null && client.Client.Connected)
                {
                    client.Client.Dispose();
                    client.Dispose();
                }
            }
            catch
            {
            }

            try
            {
                client.Dispose();
            }
            catch
            {
            }

            client = new TcpClient();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private byte[] TcpKeepAlive(bool On_Off, uint KeepaLiveTime, uint KeepaLiveInterval)
        {
            var InValue = new byte[12];

            Array.ConstrainedCopy(BitConverter.GetBytes(Convert.ToUInt32(On_Off)), 0, InValue, 0, 4);
            Array.ConstrainedCopy(BitConverter.GetBytes(KeepaLiveTime), 0, InValue, 4, 4);
            Array.ConstrainedCopy(BitConverter.GetBytes(KeepaLiveInterval), 0, InValue, 8, 4);

            return InValue;
        }

        private void doConnect()
        {
            var usernamePassword = remoteUri.UserInfo;
            var userpass2 = Uri.UnescapeDataString(usernamePassword);
            var auth = "Authorization: Basic " +
                       Convert.ToBase64String(new ASCIIEncoding().GetBytes(userpass2)) + "\r\n";

            if (usernamePassword == "")
                auth = "";

            host = remoteUri.Host;
            Port = remoteUri.Port.ToString();

            client = new TcpClient(host, int.Parse(Port));
            try
            {
                // fails under mono
                client.Client.IOControl(IOControlCode.KeepAliveValues, TcpKeepAlive(true, 36000000, 3000), null);
            }
            catch
            {
            }

            if (Port == "443" || remoteUri.Scheme == "https")
            {
                var ssl = new SslStream(client.GetStream(), false);
                ssl.AuthenticateAsClient(host);
                st = ssl;
            }
            else
            {
                st = client.GetStream();
            }

            var sw = new StreamWriter(st);
            var sr = new StreamReader(st);

            var linev1 = "GET " + remoteUri.PathAndQuery + " HTTP/1.0\r\n"
                         + "User-Agent: NTRIP MissionPlanner/1.0\r\n"
                         + auth
                         + "Connection: close\r\n\r\n";

            var linev2 = "GET " + remoteUri.PathAndQuery + " HTTP/1.1\r\n"
                         + "Host: " + remoteUri.Host + ":" + remoteUri.Port + "\r\n"
                         + "Ntrip-Version: Ntrip/2.0\r\n"
                         + "User-Agent: NTRIP MissionPlanner/1.0\r\n"
                         + auth
                         + "Connection: close\r\n\r\n";
            if (ntrip_v1)
            {
                sw.Write(linev1);
                log.Info(linev1);
            }
            else
            {
                sw.Write(linev2);
                log.Info(linev2);
            }

            sw.Flush();

            var line = sr.ReadLine();

            log.Info(line);

            if (!line.Contains("200"))
            {
                client.Dispose();

                client = new TcpClient();

                throw new Exception("Bad ntrip Responce\n\n" + line);
            }

            if (line.Contains("SOURCETABLE"))
            {
                log.Info(sr.ReadToEnd());

                client.Dispose();

                client = new TcpClient();

                throw new Exception("Got SOURCETABLE - Bad ntrip mount point\n\n" + line);
            }

            // vrs may take up to 60+ seconds to respond
            SendNMEA();

            VerifyConnected();
        }

        private void SendNMEA()
        {
            if (lat != 0 || lng != 0)
                if (_lastnmea.AddSeconds(30) < DateTime.Now)
                {
                    var latdms = (int)lat + (lat - (int)lat) * .6f;
                    var lngdms = (int)lng + (lng - (int)lng) * .6f;

                    var line = string.Format(CultureInfo.InvariantCulture,
                        "$GP{0},{1:HHmmss.ff},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", "GGA",
                        DateTime.Now.ToUniversalTime(),
                        Math.Abs(latdms * 100).ToString("0000.00", CultureInfo.InvariantCulture), lat < 0 ? "S" : "N",
                        Math.Abs(lngdms * 100).ToString("00000.00", CultureInfo.InvariantCulture), lng < 0 ? "W" : "E",
                        1, 10,
                        1, alt.ToString("0.00", CultureInfo.InvariantCulture), "M", 0, "M", "0.0", "0");

                    var checksum = GetChecksum(line);
                    WriteLine(line + "*" + checksum);

                    log.Info(line + "*" + checksum);

                    _lastnmea = DateTime.Now;
                }
        }

        // Calculates the checksum for a sentence
        private string GetChecksum(string sentence)
        {
            // Loop through all chars to get a checksum
            var Checksum = 0;
            foreach (var Character in sentence)
                switch (Character)
                {
                    case '$':
                        // Ignore the dollar sign
                        break;

                    case '*':
                        // Stop processing before the asterisk
                        continue;
                    default:
                        // Is this the first value for the checksum?
                        if (Checksum == 0)
                            Checksum = Convert.ToByte(Character);
                        else
                            Checksum = Checksum ^ Convert.ToByte(Character);
                        break;
                }

            // Return the checksum formatted as a two-character hexadecimal
            return Checksum.ToString("X2");
        }

        private void VerifyConnected()
        {
            if (!IsOpen)
            {
                try
                {
                    client.Dispose();
                    client = new TcpClient();
                }
                catch
                {
                }

                // this should only happen if we have established a connection in the first place
                if (client != null && retrys > 0)
                {
                    log.Info("ntrip reconnect");
                    doConnect();
                    retrys--;
                }

                throw new Exception("The ntrip is closed");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                Close();
                client = null;
            }

            // free native resources
        }
    }
}