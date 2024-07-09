using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace System.Net.Sockets.Kcp.Simple
{
    /// <summary>
    /// 简单例子
    /// </summary>
    public class SimpleKcpClient : IKcpCallback
    {
        UdpClient client;

        public SimpleKcpClient(uint conv, int port)
            : this(conv, port, null)
        {

        }

        public SimpleKcpClient(uint conv, int port, IPEndPoint endPoint)
        {
            client = new UdpClient(port);
            kcp = new SimpleSegManager.Kcp(conv, this);
            this.EndPoint = endPoint;
            BeginRecv();
        }

        public SimpleSegManager.Kcp kcp { get; }
        public IPEndPoint EndPoint { get; set; }

        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            client.SendAsync(s, s.Length, EndPoint);
            buffer.Dispose();
        }

        public async void SendAsync(byte[] datagram, int bytes)
        {
            kcp.Send(datagram.AsSpan().Slice(0, bytes));
        }

        public async ValueTask<byte[]> ReceiveAsync()
        {
            var (buffer, avalidLength) = kcp.TryRecv();
            while (buffer == null)
            {
                await Task.Delay(10);
                (buffer, avalidLength) = kcp.TryRecv();
            }
            var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            return s;
        }

        private async void BeginRecv()
        {
            var res = await client.ReceiveAsync();
            EndPoint = res.RemoteEndPoint;
            kcp.Input(res.Buffer);
            BeginRecv();
        }
    }
}


