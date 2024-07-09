using System;
using System.Net;
using System.Net.Sockets.Kcp.Simple;
using System.Threading.Tasks;
using LockStep.Network;
using TestFrameWork.Utils;
using UnityEngine;

namespace TestFrameWork.Server
{
    public class KcpClient : Client
    {
        private SimpleKcpClient kcpClient;
        public KcpClient(byte playerId, uint conv, int local_port, int remote_port, int cmd_count, string input_path, string output_path, Recorder recorder): base(playerId, cmd_count, input_path, recorder)
        {
            IPEndPoint end = new IPEndPoint(IPAddress.Loopback, remote_port);
            kcpClient = new SimpleKcpClient(conv, local_port, end);
            kcpClient.kcp.NoDelay(1, 10, 2, 1);
            
            Task.Run(async () =>
            {
                while (true)
                {
                    kcpClient.kcp.Update(DateTimeOffset.UtcNow);
                    await Task.Delay(10);
                }
            });
            
            StartReceive();
            //send_logger = new Logger(path + "/client_" + playerId.ToString() + "_send.txt");
            //recv_logger = new Logger(path + "/client_" + playerId.ToString() + "_recv.txt");
        }
        
        public override void Send()
        {       
            bool isSendBufferChanged;
            netWriter = codec.ReadFromSendQueue(out isSendBufferChanged);
            byte[] temp = netWriter.ToArray();
            if (temp.Length != 0)
            {
                kcpClient.SendAsync(temp, temp.Length);
                recorder.Record(playerId, viewFrameId, Stage.client_send);
                codec.ClearCache();
            }
        }

        public override ValueTask<byte[]> Receive()
        {
            var res = kcpClient.ReceiveAsync();
            return res;
        }
    }
}