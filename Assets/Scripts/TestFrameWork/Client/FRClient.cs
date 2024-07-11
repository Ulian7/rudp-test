using System.Collections.Generic;
using System.Threading.Tasks;
using TestFrameWork.Utils;
using UnityEngine;

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
        private float SRTT = 0;
        private float alpha = 0.125f;
        private Dictionary<ushort, float> lastSent;
        private Dictionary<ushort, float> firstSent;
        private Dictionary<ushort, float> threshold;
        private float StartTime;
        private float LaunchTime;

        public FRClient(byte playerId, int local_port, int remote_port, int cmd_count, string input_path, string output_path, Recorder recorder) : base(playerId, cmd_count, input_path, recorder)
        {
            udpClient = new UDPClient(local_port, remote_port);
            ACK_list = new Dictionary<ushort, bool>();
            package_queue = new Queue<byte[]>();
            lastSent = new Dictionary<ushort, float>();
            firstSent = new Dictionary<ushort, float>();
            threshold = new Dictionary<ushort, float>();
            StartTime = Time.time;
            LaunchTime = 10;
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
                        ushort last = (ushort)(package[1] + (ushort)package[0] * 256);
                        if (Time.time - StartTime <= LaunchTime)
                        {
                            udpClient.Send(package);
                            lastSent[last] = Time.time;
                            recorder.RecordSend(playerId);
                        }
                        else if (Time.time - lastSent[last] >= threshold[last])
                        {
                            udpClient.Send(package);
                            lastSent[last] = Time.time;
                            threshold[last] = Mathf.Min(SRTT, threshold[last] * 2);
                            recorder.RecordSend(playerId);
                        }
                    }
                }
            }

            if (temp.Length > HEADER)
            {
                //Debug.Log("Send SEQ " + SEQ.ToString() + " ACK " + ACK.ToString() + " SID " + playerId.ToString());
                ushort last = SEQ;
                SEQ++;
                udpClient.Send(temp);
                recorder.RecordSend(playerId);
                recorder.Record(playerId, viewFrameId, Stage.client_send);
                codec.ClearCache();
                lock (package_queue)
                {
                    package_queue.Enqueue(temp);
                    lastSent[last] = Time.time;
                    firstSent[last] = Time.time;
                    threshold[last] = SRTT == 0 ? 0.03f : Mathf.Min(0.03f, SRTT / 4); 
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
                    float RTT = Time.time - firstSent[UAC];
                    SRTT = SRTT == 0 ? RTT : (1 - alpha) * SRTT + alpha * RTT;
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