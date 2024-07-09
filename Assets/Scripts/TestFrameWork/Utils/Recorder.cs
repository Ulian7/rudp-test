using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TestFrameWork.Utils
{   
    public class Recorder
    {
        private Dictionary<int, timestamp[]> timeline;
        public float[] send_count;
        private string csvPath;
        private int frames;
        public float startTime;
        private Logger logger;
        

        public Recorder(string path, int client_num, int frames)
        {
            csvPath = "csv/" + path;
            startTime = Time.time;
            this.frames = frames;
            send_count = new float[client_num];
            timeline = new Dictionary<int, timestamp[]>();
            for (int i = 0; i < client_num; i++)
            {
                timeline[i] = new timestamp[frames];
            }
        }

        public void RecordSend(byte playerId)
        {
            send_count[playerId]++;
        }
        
        public void Record(byte playerId, int viewFrameId, Stage stage)
        {
            switch (stage)
            {
                case Stage.client_send:
                    timeline[playerId][viewFrameId].client_send = (int)((Time.time - startTime) * 1000);
                    break;
                case Stage.server_recv:
                    timeline[playerId][viewFrameId].server_recv = (int)((Time.time - startTime) * 1000);
                    break;
                case Stage.server_send:
                    timeline[playerId][viewFrameId].server_send = (int)((Time.time - startTime) * 1000);
                    break;
                case Stage.client_recv:
                    timeline[playerId][viewFrameId].client_recv = (int)((Time.time - startTime) * 1000);
                    break;
                case Stage.client_handle:
                    timeline[playerId][viewFrameId].client_handle = (int)((Time.time - startTime) * 1000);
                    break;
            }
        }
        
        public void WriteToCSV()
        {
            if (!Directory.Exists(csvPath))
                Directory.CreateDirectory(csvPath);
            logger = new Logger(csvPath + "/redundant_rate.csv");
            for (int i = 0; i < send_count.Length; i++)
            {
                float rate = send_count[i] / frames;
                logger.WriteIntoLog("Player" + i.ToString() + "," + rate.ToString());
            }
            for (int i = 0; i < timeline.Count; i++)
            {
                logger = new Logger(csvPath + "/client_" + i.ToString() + ".csv");
                logger.WriteIntoLog("序号,发送->接收,接收->转发,转发->接收,接收->处理,总计");
                for (int j = 0; j < timeline[i].Length; j++)
                {
                    timestamp temp = timeline[i][j];
                    if (temp.client_send == 0)
                    {
                        continue;
                    }
                    logger.WriteIntoLog(j.ToString() + "," + (temp.server_recv - temp.client_send).ToString() + "," +
                                        (temp.server_send - temp.server_recv).ToString() + "," +
                                        (temp.client_recv - temp.server_send).ToString() + "," +
                                        (temp.client_handle - temp.client_recv).ToString() + "," +
                                        (temp.client_handle - temp.client_send).ToString());
                }
            }
        }
    }
}