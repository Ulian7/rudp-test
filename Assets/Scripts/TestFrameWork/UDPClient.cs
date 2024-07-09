using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TestFrameWork.Utils
{
    public class UDPClient
    {
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;
        private Queue<byte> buffer;
        private Queue<int> package_length;
        public UDPClient(int local_port, int remote_port)
        {
            udpClient = new UdpClient(local_port);
            remoteEndPoint = new IPEndPoint(IPAddress.Loopback, remote_port);
            buffer = new Queue<byte>();
            package_length = new Queue<int>();
            Listen();
        }

        ~UDPClient()
        {
            udpClient.Close();
        }
        public void Send(byte[] buffer)
        {
            udpClient.Send(buffer, buffer.Length, remoteEndPoint);
        }

        public async void Listen()
        {
            IPEndPoint re = new IPEndPoint(IPAddress.Any, 0);
            await Task.Run(async () =>
            {
                while (true)
                {
                    byte[] temp = udpClient.Receive(ref re);
                    lock (package_length)
                    {
                        lock (buffer)
                        {
                            package_length.Enqueue(temp.Length);
                            foreach (byte b in temp)
                            {
                                buffer.Enqueue(b);
                            }
                        }
                    }
                }
            });
        }

        public byte[] TryRecv()
        {
            byte[] res;
            if (package_length.Count == 0)
            {
                return null;
            }
            lock (package_length)
            {
                res = new byte[package_length.Dequeue()];
                lock (buffer)
                {
                    for (int i = 0; i < res.Length; i++)
                    {
                        res[i] = buffer.Dequeue();
                    }
                }
            }
            return res;
        }
        public async ValueTask<byte[]> ReceiveAsync()
        {
            byte[] temp = TryRecv();
            while (temp == null)
            {
                await Task.Delay(10);
                temp = TryRecv();
            }
            return temp;
        }
    }
}