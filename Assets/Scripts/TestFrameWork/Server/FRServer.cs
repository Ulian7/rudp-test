using LockStep.Network;
using System.Collections.Generic;
using TestFrameWork.Utils;

namespace TestFrameWork.Server
{
    public class FRServer : Server
    {
        private const int HEADER = 6;
        private ushort SEQ = 1;
        private ushort[] UAC;
        private ushort[] ACK;
        private List<ushort>[] ACK_History;
        private Dictionary<ushort, bool>[] ACK_list;
        private List<byte[]> package_queue;
        private List<UDPClient> client_list;

        public FRServer(int local_port, int remote_port, int client_num, string output_path, Recorder recorder) : base(client_num, recorder)
        {
            UAC = new ushort[client_num];
            ACK = new ushort[client_num];
            ACK_History = new List<ushort>[client_num];
            client_list = new List<UDPClient>();
            ACK_list = new Dictionary<ushort, bool>[client_num];
            package_queue = new List<byte[]>();
            for (int i = 0; i < client_num; i++)
            {
                UAC[i] = 1;
                ACK[i] = 1;
                ACK_History[i] = new List<ushort>();
                ACK_list[i] = new Dictionary<ushort, bool>();
                client_list.Add(new UDPClient(local_port+i, remote_port+i));
            }
            ListenOnClients();
        }
        
        public override void HeadHandler()
        {
            ushort temp = 0;
            netWriter.Write(SEQ++);
            netWriter.Write(temp);
            netWriter.Write(temp);
        }

        public void ChangeHead(byte[] buffer, ushort ack, ushort playerId)
        {
            buffer[2] = (byte) ((ack >> 8) & 0xff);
            buffer[3] = (byte) (ack & 0xff);
            buffer[4] = (byte) ((playerId >> 8) & 0xff);
            buffer[5] = (byte) (playerId & 0xff);
        }
        public override void Send()
        {
            byte[] temp = netWriter.ToArray();
            temp[HEADER] = frame_count;
            NetReader netReader = new NetReader(temp);
            _ = netReader.ReadUInt16();
            _ = netReader.ReadUInt16();
            _ = netReader.ReadUInt16();
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
            for (ushort i = 0; i< client_list.Count;i++)
            {
                ushort uac = UAC[i];
                ushort ack = ACK[i];
                for (ushort j = --uac; j < package_queue.Count; j++)
                {
                    ChangeHead(package_queue[j], ACK_History[i][j],i);
                    client_list[i].Send(package_queue[j]);
                }

                ChangeHead(temp, ack, i);
                client_list[i].Send(temp);
                ACK_History[i].Add(ack);
            }
            package_queue.Add(temp);
        }
        public override void ListenOnClients()
        {
            foreach (UDPClient client in client_list)
            {
                StartReceive(client);
            }
        }

        public async void StartReceive(UDPClient client)
        {
            while (is_running)
            {
                var res = await client.ReceiveAsync();
                netReader = new NetReader(res);
                ushort seq = netReader.ReadUInt16();
                ushort ack = netReader.ReadUInt16();
                byte playerId = (byte)netReader.ReadUInt16();
                //Debug.Log("Recv SEQ " + seq.ToString() + " ACK " + ack.ToString() + " playerId " + playerId.ToString());
                SyncCmd tempCmd = new SyncCmd();
                byte CmdCount = netReader.ReadByte();
                tempCmd.ReadFromBuffer(netReader);

                if (ACK_list[playerId].ContainsKey(seq))
                {
                    continue;
                }
                
                ACK_list[playerId][seq] = true;
                UAC[playerId] = ++ack;
                
                while (ACK_list[playerId].ContainsKey((ushort)(ACK[playerId]+1)))
                {
                    ACK[playerId]++;
                }
                recorder.Record(playerId, tempCmd.ClientFrameId, Stage.server_recv);
                
                bool newList;
                List<SyncCmd> CmdList;
                lock (Map)
                {
                    newList = !Map.ContainsKey(playerId);
                    CmdList = newList ? new List<SyncCmd>() : Map[playerId];
                }
                CmdList.Add(tempCmd);
                
                for (int i = 1; i < CmdCount; i++)
                {
                    tempCmd.ReadFromBuffer(netReader);
                    recorder.Record(playerId, tempCmd.ClientFrameId, Stage.server_recv);
                    CmdList.Add(tempCmd);
                }
                
                if (newList)
                    lock (Map)
                    {
                        Map[playerId] = CmdList;
                    }
            }
        }
    }
}