using System.Collections.Generic;
using TestFrameWork.Server;
using TestFrameWork.Utils;
using UnityEditor;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public Strategy strategy;
    public int lag;
    public int drop;
    public int client_num = 2;
    public int logic_fps = 30;
    public int view_fps = 30;
    public int server_fps = 15;
    public int seconds = 180;
    public int cmd_count = 2;
    public uint conv = 2001;
    public int local_port = 50001;
    public int remote_port = 40001;

    private float client_accumulator;
    private float server_accumulator;
    private float logic_interval;
    private float server_interval;
    private string input_path;
    private string output_path;
    private bool flag;
    
    private Recorder recorder;
    private Server server;
    private List<Client> clients;
    
    // Start is called before the first frame update
    void Start()
    {
        client_accumulator = 0;
        server_accumulator = 0;
        logic_interval = 1.0f / logic_fps;
        server_interval = 1.0f / server_fps;
        output_path = strategy + "/lfps_" + logic_fps.ToString() + "_vfps_" + view_fps.ToString() + "_sfps_" + server_fps.ToString() +
               "_seconds_" + seconds.ToString() + "/lag_" + lag.ToString() + "_drop_" + drop.ToString() + "%";
        recorder = new Recorder(output_path, client_num, view_fps * seconds);
        clients = new List<Client>();

        for (int i = 0; i < client_num; i++)
        {
            input_path = "python_scripts/sample_" + i.ToString() + "_vfps_" + view_fps.ToString() + "_seconds_" +
                         seconds.ToString() + ".txt";
            switch (strategy)
            {
                case Strategy.kcp:
                    clients.Add(new KcpClient((byte)i, conv + (uint)i, local_port + i, remote_port + i, cmd_count,
                        input_path, output_path, recorder));
                    break;
                case Strategy.FR:
                    clients.Add(new FRClient((byte)i, local_port + i, remote_port + i, cmd_count, input_path, output_path,
                        recorder));
                    break;
            }
        }
        switch (strategy)
        {
            case Strategy.kcp:
                server = new KcpServer(conv, remote_port, client_num, output_path, recorder);
                break;
            case Strategy.FR:
                server = new FRServer(remote_port, local_port, client_num, output_path, recorder);
                break;
        }
        Application.targetFrameRate = view_fps;
    }

    // Update is called once per frame
    void Update()
    {
        client_accumulator += Time.deltaTime;
        server_accumulator += Time.deltaTime;
        for (int i = 0; i < client_num; i++)
        {
            clients[i].ViewTick();
        }

        flag = true;
        while (client_accumulator >= logic_interval)
        {
            int temp;
            for (int i = 0; i < client_num; i++)
            {
                temp = clients[i].LogicTick();
                if (temp < logic_fps * (seconds + 10))
                {
                    flag = false;
                }
            }
            if (flag)
               EditorApplication.ExitPlaymode();
            server.LogicTick();
            client_accumulator -= logic_interval;
        }

        while (server_accumulator >= server_interval)
        {
            server.BroadcastTick();
            server_accumulator -= server_interval;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < client_num; i++)
        {
            clients[i].StopReceive();
        }
        server.StopReceive();
    }

    private void OnApplicationQuit()
    {
        if (flag) 
            recorder.WriteToCSV();
    }
}
