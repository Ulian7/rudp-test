using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Main : MonoBehaviour
{
    public int client_num = 2;
    public int client_fps = 30;
    public int server_fps = 15;
    public uint conv = 2001;
    public int send_port = 50001;
    public int end_port = 40001;
    
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.AddComponent<ResMgr>();
        GameObject serverPrefab = ResMgr.Instance.GetAssetCache<GameObject>("Prefab/server.prefab");
        GameObject serverObj = GameObject.Instantiate(serverPrefab);
        serverObj.name = "Server";
        Server server = serverObj.GetComponent<Server>();
        server.client_num = client_num;
        server.fps = server_fps;

        GameObject clientObj;
        Client client;
        GameObject clientPrefab = ResMgr.Instance.GetAssetCache<GameObject>("Prefab/Client.prefab");
        for (int i = 0; i < client_num; i++)
        {
            clientObj = GameObject.Instantiate(clientPrefab);
            clientObj.name = "Client_" + i.ToString();
            client = clientObj.GetComponent<Client>();
            client.log_path = "rudp_Logs/client_" + i.ToString() + ".txt";
            client.sample_path = "python_scripts/sample_" + i.ToString() + ".txt";
            client.res_path = "python_scripts/lag_" + i.ToString() + ".txt";
            client.fps = client_fps;
            client.playerId = (byte)i;
            client.conv = conv + (uint)i;
            client.send_port = send_port + i;
            client.end_port = end_port + i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
