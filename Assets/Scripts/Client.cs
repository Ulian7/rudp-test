using Morefun.LockStep.Network;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string log_path;
    public string sample_path;
    public string res_path;
    public int fps = 15;
    private Logger logger;
    private float interval;
    private Codec codec;
    private float addUpTime;
    private ushort currentFrameId;
    private IPEndPoint end;
    private string sample_string;
    private int sample_length;
    private int receive_count;
    private int input_sum;
    private GameObject text;
    
    private Proto_Base client;
    
    public byte playerId = 1;
    public ushort vkey = 32767;
    public byte[] args = { 100, 173, 7 };
    public uint conv = 2001;
    public int send_port = 50001;
    public int end_port = 40001;
    
    private float[] send_list;
    private float[] receive_list;
    void Start()
    {
        interval = Mathf.Floor(1000 / fps);
        addUpTime = 0;
        currentFrameId = 0;
            
        logger = new Logger(log_path);
        codec = new Codec();
        end =  new IPEndPoint(IPAddress.Loopback, end_port);
        sample_string = File.ReadAllText(sample_path);
        sample_length = sample_string.Length;
        send_list = new float[sample_length];
        receive_list = new float[sample_length];
        receive_count = 0;
        input_sum = 0;

        text = GameObject.Find("Text");
        
        for (int i = 0; i < sample_length; i++)
            if (sample_string[i] != '0')
            {
                input_sum++;
            }
        client = new Kcp(conv, send_port, end);
        Task.Run(async () =>
        {
            while (true)
            {
                client.Update(DateTimeOffset.UtcNow);
                StartRecv(client);
                await Task.Delay(10);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        addUpTime += Time.deltaTime;
        if (Mathf.Floor(addUpTime * 1000 / interval) > currentFrameId)
        {
            if (currentFrameId < sample_length && sample_string[currentFrameId] != '0')
            {
                codec.TransToFrameCmd(playerId, Convert.ToUInt16(sample_string[currentFrameId]), currentFrameId, args);
                bool isSendBufferChanged;
                NetWriter netWriter = codec.ReadFromSendQueue(out isSendBufferChanged);

                Byte[] temp = netWriter.ToArray();
                send_list[currentFrameId] = addUpTime * 1000;
                client.Send(temp);
            }

            currentFrameId = (ushort)Mathf.Floor(addUpTime * 1000 / interval);
            codec.ClearCache();
        }

        if (receive_count == input_sum)
        {
            text.SetActive(false);
            Application.Quit();
        }
    }
    async void StartRecv(Proto_Base client)
    {
        var res = await client.Receive(interval);
        
        if (res.Length != 0)
        {
            //Debug.Log("Logs");
            SyncFrame syncFrame = new SyncFrame();
            syncFrame.ReadFromBuffer(new NetReader(res));
            foreach (SyncCmd cmd in syncFrame.CmdList)
            {
                if (cmd.PlayerId == playerId)
                {
                    receive_list[cmd.ClientFrameId] = addUpTime * 1000;
                    receive_count++;
                }
            }
            logger.WriteIntoLog("ClientFrameId: " + currentFrameId.ToString());
            logger.WriteIntoLog(syncFrame.ToString());
        }
    }

    private void OnApplicationQuit()
    {
        Logger logger = new Logger(res_path);
        for (int i = 0; i < sample_length; i++)
            if (sample_string[i] != '0')
            {
                float lag = (receive_list[i] - send_list[i]);
                logger.WriteIntoLog(lag.ToString());
            }
    }
}
