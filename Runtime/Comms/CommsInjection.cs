using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MAVLinkAPI.Comms
{
    /// <summary>
    /// use AppendBuffer to populate the Read buffer, and WriteCallback to send
    /// </summary>
    public class CommsInjection : ICommsSerial
    {
        private readonly CircularBuffer<byte> _bufferRX = new(1024 * 100);

        public CommsInjection()
        {
            BaseStream = new CommsStream(this, 0);
        }

        public void AppendBuffer(byte[] indata)
        {
            lock (_bufferRX)
            {
                foreach (var b in indata) _bufferRX.Add(b);

                BaseStream.SetLength(BaseStream.Length + indata.Length);
            }
        }

        public EventHandler<int> ReadBufferUpdate;
        public EventHandler<IEnumerable<byte>> WriteCallback;

        public void Close()
        {
            lock (_bufferRX)
            {
                _bufferRX.Clear();
            }
        }

        public void DiscardInBuffer()
        {
            lock (_bufferRX)
            {
                _bufferRX.Clear();
            }
        }

        public void Open()
        {
            lock (_bufferRX)
            {
                _bufferRX.Clear();
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var counttimeout = 0;
            while (BytesToRead == 0)
            {
                Thread.Sleep(1);
                if (counttimeout > ReadTimeout)
                    throw new Exception("CommsInjection Timeout on read");
                counttimeout++;
            }

            lock (_bufferRX)
            {
                var read = Math.Min(count, _bufferRX.Length());
                for (var i = 0; i < read; i++) buffer[offset + i] = _bufferRX.Read();

                return read;
            }
        }

        public int ReadByte()
        {
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
            var data = new byte[0];
            if (data.Length > 0)
                Read(data, 0, data.Length);

            var line = Encoding.ASCII.GetString(data, 0, data.Length);

            return line;
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

        public void WriteLine(string line)
        {
            line = line + "\n";
            Write(line);
        }

        public void Write(string line)
        {
            var data = new ASCIIEncoding().GetBytes(line);
            Write(data, 0, data.Length);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            WriteCallback?.Invoke(this, buffer.Skip(offset).Take(count));
        }

        public void toggleDTR()
        {
        }

        public void Dispose()
        {
            Close();
        }

        public Stream BaseStream { get; }
        public int BaudRate { get; set; }

        public int BytesToRead
        {
            get
            {
                ReadBufferUpdate?.Invoke(this, 0);
                lock (_bufferRX)
                {
                    return _bufferRX.Length();
                }
            }
        }

        public int BytesToWrite { get; }
        public int DataBits { get; set; }
        public bool DtrEnable { get; set; }

        public bool IsOpen => true;

        public string PortName { get; set; }
        public int ReadBufferSize { get; set; }
        public int ReadTimeout { get; set; }
        public bool RtsEnable { get; set; }

        public int WriteBufferSize { get; set; }
        public int WriteTimeout { get; set; }
    }
}