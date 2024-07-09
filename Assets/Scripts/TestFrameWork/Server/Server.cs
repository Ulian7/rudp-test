using LockStep.Network;
using System.Collections.Generic;
using TestFrameWork.Utils;
namespace TestFrameWork.Server
{
    public class Server
    {
        public int client_num = 0;
        
        private int logicFrameId = 0;
        private int serverFrameId = 0;
        public byte frame_count = 0;
        public bool is_running = false;
        public NetReader netReader;
        public NetWriter netWriter;
        public Recorder recorder;
        public SyncFrame syncFrame;
        
        private Codec codec;
        public Dictionary<int, List<SyncCmd>> Map;
        public Server(int client_num, Recorder recorder)
        {
            this.client_num = client_num;
            this.recorder = recorder;
            netWriter = new NetWriter();
            codec = new Codec();
            syncFrame = new SyncFrame();
            Map = new Dictionary<int, List<SyncCmd>>();
            is_running = true;
            HeadHandler();
            netWriter.Write(frame_count);
        }
        
        public virtual void Send()
        {
        }

        public virtual void HeadHandler()
        {
        }
        
        public void LogicTick()
        {
            frame_count++;
            syncFrame.FrameId = (ushort)serverFrameId;
            lock (Map)
            {
                for (int i = 0; i < client_num; i++)
                    if (Map.ContainsKey(i))
                    {
                        foreach (SyncCmd Cmd in Map[i])
                            syncFrame.CmdList.Add(Cmd);
                    }
                Map.Clear();
            }
            syncFrame.WriteToBuffer(netWriter);
            syncFrame.CmdList.Clear();
            logicFrameId++;
        }
        public void BroadcastTick()
        {
            Send();
            netWriter.SeekZero();
            frame_count = 0;
            HeadHandler();
            netWriter.Write(frame_count);
            serverFrameId++;
        }
        
        public void StopReceive()
        {
            is_running = false;
        }

        public virtual async void ListenOnClients()
        {
            
        }
    }
}