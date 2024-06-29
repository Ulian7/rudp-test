using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KF;
public class SetupSocket : MonoBehaviour
{
    // Start is called before the first frame update
    UDPSocket udpSocket;
    void Start()
    {
        udpSocket = new UDPSocket();
        udpSocket.Connect("127.0.0.1", "", 5000);
        byte[] temp = {85, 108, 105, 97, 110};
        udpSocket.SendTo(temp, 5);
        udpSocket.Close();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
