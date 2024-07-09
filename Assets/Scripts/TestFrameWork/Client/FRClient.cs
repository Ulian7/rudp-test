using System.Collections.Generic;
using System.Threading.Tasks;
using TestFrameWork.Utils;

namespace TestFrameWork.Server
{
    public class FRClient : Client
    {
        private const int HEADER = 6;
        private ushort SEQ = 1;
        private ushort UAC = 1;
        private ushort ACK = 0;
        private Dictionary<ushort, bool> ACK_list;
        private Queue<byte[]> package_queue;
        private UDPClient udpClient;

        public FRClient(byte playerId, int local_port, int remote_port, int cmd_count, string input_path, string output_path, Recorder recorder) : base(playerId, cmd_count, input_path, recorder)
        {
            udpClient = new UDPClient(local_port, remote_port);
            ACK_list = new Dictionary<ushort, bool>();
            package_queue = new Queue<byte[]>();
            StartReceive();
        }

        public override void Send()
        {
            bool isSendBufferChanged;
            netWriter = codec.ReadFromSendQueue(out isSendBufferChanged, true, SEQ, ACK, (ushort)playerId);
            byte[] temp = netWriter.ToArray();
            lock (package_queue)
            {
                if (package_queue.Count > 0)
                {
                    foreach (byte[] package in package_queue)
                    {
                        udpClient.Send(package);
                        recorder.RecordSend(playerId);
                    }
                }
            }

            if (temp.Length > HEADER)
            {
                //Debug.Log("Send SEQ " + SEQ.ToString() + " ACK " + ACK.ToString() + " SID " + playerId.ToString());
                SEQ++;
                udpClient.Send(temp);
                recorder.RecordSend(playerId);
                recorder.Record(playerId, viewFrameId, Stage.client_send);
                codec.ClearCache();
                lock (package_queue)
                {
                    package_queue.Enqueue(temp);
                }
            }
        }

        public override bool HeadHandler()
        {
            ushort seq = netReader.ReadUInt16();
            ushort ack = netReader.ReadUInt16();
            netReader.ReadUInt16();
            //Debug.Log("Recv SEQ " + seq.ToString() + " ACK " + ack.ToString() + " playerId " + playerId.ToString());
            if (ACK_list.ContainsKey(seq))
            {
                return false;
            }
            
            ACK_list[seq] = true;
            
            for (; UAC <= ack; UAC++)
            {
                lock (package_queue)
                {
                    package_queue.Dequeue();
                }
            }
            
            while (ACK_list.ContainsKey((ushort)(ACK+1)))
            {
                ACK++;
            }
            return true;
        }

        public override ValueTask<byte[]> Receive()
        {
            var res = udpClient.ReceiveAsync();
            return res;
        }
    }
}