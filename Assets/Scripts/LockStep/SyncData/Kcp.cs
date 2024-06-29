using System;
using System.Net;
using System.Net.Sockets.Kcp.Simple;
using System.Threading.Tasks;
using Morefun.LockStep.Network;

public class Kcp : Proto_Base
{
    private SimpleKcpClient kcpClient;

    public Kcp(uint conv, int port, IPEndPoint end)
    {
        kcpClient = new SimpleKcpClient(conv, port, end);
        kcpClient.kcp.NoDelay(1, 10, 2, 1);
    }

    public Kcp(uint conv, int port)
    {
        kcpClient = new SimpleKcpClient(conv, port);
        kcpClient.kcp.NoDelay(1, 10, 2, 1);
    }

    public override void Update(in DateTimeOffset time)
    {
        kcpClient.kcp.Update(time);
    }

    public override void Send(byte[] bytes)
    {
        kcpClient.SendAsync(bytes, bytes.Length);
    }

    public override async ValueTask<Byte[]> Receive(float interval)
    {
        var res = await kcpClient.ReceiveAsync(interval);
        return res;
    }
}
