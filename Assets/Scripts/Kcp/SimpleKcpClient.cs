using System.Buffers;
using UnityEngine;
using System.Threading.Tasks;
using TestFrameWork.Utils;

namespace System.Net.Sockets.Kcp.Simple
{
    /// <summary>
    /// 简单例子
    /// </summary>
    public class SimpleKcpClient : IKcpCallback
    {
        UdpClient client;
        private bool is_player = false;
        private byte playerId;
        private Recorder recorder;
        public SimpleKcpClient(uint conv, int port)
            : this(conv, port, null)
        {

        }

        public SimpleKcpClient(uint conv, int port, IPEndPoint endPoint, byte playerId = 0, Recorder recorder = null)
        {
            this.playerId = playerId;
            client = new UdpClient(port);
            kcp = new SimpleSegManager.Kcp(conv, this);
            this.EndPoint = endPoint;
            this.recorder = recorder;
            BeginRecv();
        }

        public SimpleSegManager.Kcp kcp { get; }
        public IPEndPoint EndPoint { get; set; }

        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            client.Send(s, s.Length, EndPoint);
            if (recorder != null)
            {
                recorder.RecordSend(playerId);
            }
            buffer.Dispose();
        }

        public void SendAsync(byte[] datagram, int bytes)
        {
            kcp.Send(datagram.AsSpan().Slice(0, bytes));
        }

        public async ValueTask<byte[]> ReceiveAsync()
        {
            var (buffer, avalidLength) = kcp.TryRecv();
            while (buffer == null)
            {
                await Task.Delay(1);
                (buffer, avalidLength) = kcp.TryRecv();
            }
            var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            return s;
        }

        private async void BeginRecv()
        {
            IPEndPoint re = new IPEndPoint(IPAddress.Any, 0);
            await Task.Run(async () =>
            {
                while (true)
                {
                    byte[] temp = client.Receive(ref re);
                    EndPoint = re;
                    kcp.Input(temp);
                }
            });
        }
    }
}


