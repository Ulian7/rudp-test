using LockStep.Network;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TestFrameWork.Utils;
using UnityEngine;
namespace TestFrameWork.Server
{
    public class Client
    {
        public byte playerId;
        public int viewFrameId = 0;
        //private Logger send_logger;
        //private Logger recv_logger;
        public Codec codec;
        public NetWriter netWriter;
        public NetReader netReader;
        public Recorder recorder;
        
        private int logicFrameId = 0;
        private int currentFrameId = 0;

        private int cmd_count;
        private string input;
        private int input_frames;
        private byte[] args = { 192, 168, 7 };
        private bool is_running;
        private int frame_sum = 0;
        
        private Dictionary<int, List<SyncCmd>> Map;
        public Client(byte playerId, int cmd_count, string input_path, Recorder recorder)
        {
            codec = new Codec();
            this.playerId = playerId;
            this.cmd_count = cmd_count;
            this.recorder = recorder;
            Map = new Dictionary<int, List<SyncCmd>>();
            input = File.ReadAllText(input_path);
            input_frames = input.Length / cmd_count;
            is_running = true;
        }

        public virtual void Send()
        {
        }

        public virtual bool HeadHandler()
        {
            return true;
        }
        public virtual ValueTask<byte[]> Receive()
        {
            return new ValueTask<byte[]>(new byte[]{});
        }

        public void ViewTick()
        {
            if (viewFrameId < input_frames)
            {
                for (int i = 0; i < cmd_count; i++)
                {
                    if (input[viewFrameId * cmd_count + i] != '0')
                    {
                        codec.TransToFrameCmd(playerId, (ushort)(input[viewFrameId * cmd_count +i] - 48), (ushort)viewFrameId, args);
                    }
                }
                Send();
            }
            viewFrameId++;
        }

        public int LogicTick()
        {
            while (Map.ContainsKey(currentFrameId) && currentFrameId < logicFrameId)
            {
                foreach (SyncCmd Cmd in Map[currentFrameId])
                {
                    if (Cmd.PlayerId == playerId)
                    {
                        recorder.Record(playerId, Cmd.ClientFrameId, Stage.client_handle);
                    }
                }
                Debug.Log(currentFrameId);
                currentFrameId++;
            }
            logicFrameId++;
            return currentFrameId;
        }

        public void StopReceive()
        {
            is_running = false;
        }

        public async void StartReceive()
        {
            while (is_running)
            {
                var res = await Receive();
                netReader = new NetReader(res);
                if (!HeadHandler())
                {
                    continue;
                }
                byte frameCount = netReader.ReadByte();
                for (int i = 0; i < frameCount; i++)
                {
                    SyncFrame syncFrame = new SyncFrame();
                    syncFrame.ReadFromBuffer(netReader);
                    foreach (SyncCmd Cmd in syncFrame.CmdList)
                    {
                        if (Cmd.PlayerId == playerId)
                        {
                            recorder.Record(playerId, Cmd.ClientFrameId, Stage.client_recv);
                        }
                    }
                    Map[frame_sum + i] = syncFrame.CmdList;
                }
                frame_sum += frameCount;
            }
            
        }
    }
}