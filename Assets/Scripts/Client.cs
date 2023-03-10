using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Client : MonoBehaviour
{
    // Create connection to server
    // Create Thread for receiving server messages
    private IPEndPoint endPoint = null;
    private TcpClient clientConnection = null;

    public void initClient(string IP, int port)
    {
        // Create Server Endpoint
        endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        // Listens for Client Connections right away.
        Thread tcpClientThread = new Thread(new ThreadStart(ConnectionListener));
        // Sets ListenerThread as background thread.
        tcpClientThread.IsBackground = true;
        tcpClientThread.Start();
    }

    // Delegate Method
    void ConnectionListener()
    {
        clientConnection = new TcpClient(endPoint);
        byte[] buffer = new byte[1024];

        // TODO: Add exception handling
        while (true)
        {
            using (NetworkStream stream = clientConnection.GetStream())
            {
                int recv;
                while ((recv = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string recievedMessage = Encoding.ASCII.GetString(buffer);
                    System.Diagnostics.Debug.WriteLine("Recieved Message:\n" + recievedMessage);
                }
            }
        }
    }

    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        
    }
}
