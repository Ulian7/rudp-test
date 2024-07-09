using LockStep.Network;
using System;
using System.Collections.Generic;
using System.Net.Sockets.Kcp.Simple;
using System.Threading.Tasks;
using TestFrameWork.Utils;

namespace TestFrameWork.Server
{
    public class KcpServer : Server
    {
        private List<SimpleKcpClient> client_list;
        private Codec codec;
        public KcpServer(uint conv, int port, int client_num, string out_path, Recorder recorder) : base(client_num, recorder)
        {
            client_list = new List<SimpleKcpClient>();
            for (int i = 0; i < client_num; i++)
            {
                client_list.Add(new SimpleKcpClient((uint)(conv + i), port + i));
                client_list[i].kcp.NoDelay(1, 10, 2, 1);
            }

            Task.Run(async () =>
            {
                while (true)
                {
                    foreach (SimpleKcpClient client in client_list)
                    {
                        client.kcp.Update(DateTimeOffset.UtcNow);
                    }

                    await Task.Delay(10);
                }
            });
            ListenOnClients();
        }
        
        public override void Send()
        {
            byte[] temp = netWriter.ToArray();
            temp[0] = frame_count;
            NetReader netReader = new NetReader(temp);
            _ = netReader.ReadByte();
            for (int i = 0; i < frame_count; i++)
            {
                SyncFrame syncFrame = new SyncFrame();
                syncFrame.ReadFromBuffer(netReader);
                foreach (SyncCmd Cmd in syncFrame.CmdList)
                {
                    recorder.Record(Cmd.PlayerId, Cmd.ClientFrameId, Stage.server_send);
                }
            }
            foreach (SimpleKcpClient client in client_list)
            {
                client.SendAsync(temp, temp.Length);
            }
        }

        public override void ListenOnClients()
        {
            foreach (SimpleKcpClient client in client_list)
            {
                StartReceive(client);
            }
        }
        
        public async void StartReceive(SimpleKcpClient client)
        {
            while (is_running)
            {
                List<SyncCmd> CmdList;
                bool newList = false;
                var res = await client.ReceiveAsync();
                
                SyncCmd tempCmd = new SyncCmd();
                NetReader netReader = new NetReader(res);
                byte CmdCount = netReader.ReadByte();
                tempCmd.ReadFromBuffer(netReader);
                recorder.Record(tempCmd.PlayerId, tempCmd.ClientFrameId, Stage.server_recv);
                lock (Map)
                {
                    newList = !Map.ContainsKey(tempCmd.PlayerId);
                    CmdList = newList ? new List<SyncCmd>() : Map[tempCmd.PlayerId];
                }

                CmdList.Add(tempCmd);
                for (int i = 1; i < CmdCount; i++)
                {
                    tempCmd.ReadFromBuffer(netReader);
                    recorder.Record(tempCmd.PlayerId, tempCmd.ClientFrameId, Stage.server_recv);
                    CmdList.Add(tempCmd);
                }
                
                if (newList)
                    lock (Map)
                    {
                        Map[tempCmd.PlayerId] = CmdList;
                    }
            }
        }
    }
}