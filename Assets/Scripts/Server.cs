using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morefun.LockStep.Network;
using UnityEngine;


public class Server : MonoBehaviour
{
    public int client_num = 2;
    public int fps = 15;
    private float interval;
    private float addUpTime;
    private ushort currentFrameId;
    private SyncFrame syncFrame;
    private List<Proto_Base> client_list;
    private int sum;
    
    public uint conv = 2001;
    public int port = 40001;
    void Start()
    {
        interval = Mathf.Floor(1000 / fps);
        addUpTime = 0;
        currentFrameId = 0;
        syncFrame = new SyncFrame();
        client_list = new List<Proto_Base>();
        
        for (int i = 0; i < client_num; i++)
        {
            //SimpleKcpClient kcpClient = new SimpleKcpClient(conv + (uint)i, port + i);
            client_list.Add(new Kcp(conv + (uint)i, port + i));
        }
        Task.Run(async () =>
        {
            while (true)
            {
                foreach (Proto_Base client in client_list)
                {
                    client.Update(DateTimeOffset.UtcNow);
                    StartRecv(client);
                }
                
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
            syncFrame.FrameId = currentFrameId;
            if (syncFrame.CmdList.Any())
                lock (syncFrame)
                {
                    //Debug.Log("Send");
                    foreach (Proto_Base client in client_list)
                    {
                        NetWriter netWriter = new NetWriter();
                        syncFrame.WriteToBuffer(netWriter);
                        byte[] temp = netWriter.ToArray();
                        client.Send(temp);
                    }
                    syncFrame.CmdList.Clear();
                }
            currentFrameId = (ushort)Mathf.Floor(addUpTime * 1000 / interval);
        }
    }
    
    async void StartRecv(Proto_Base client)
    {
        var res = await client.Receive(interval);
        if (res.Length != 0)
        {
            lock (syncFrame)
            {
                SyncCmd tempCmd = new SyncCmd();
                tempCmd.ReadFromBuffer(new NetReader(res));
                syncFrame.CmdList.Add(tempCmd);
            }
        }
    }
}
