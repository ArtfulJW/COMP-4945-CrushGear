using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    private IPEndPoint endPoint = null;
    List<TcpClient> clientList = null;

    public void initServer(string IP, int port)
    {
        // Create Server Endpoint
        endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        // Listens for Client Connections right away.
        Thread tcpListenerThread = new Thread(new ThreadStart(ListenForClientConnections));
        // Sets ListenerThread as background thread.
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    // Delegate Method
    void ListenForClientConnections()
    {
        TcpListener tcpListener = new TcpListener(endPoint);
        tcpListener.Start();

        // TODO: Add exception handling
        while (true)
        {
            // Accept Client connections
            TcpClient client = tcpListener.AcceptTcpClient();
            // Add accepted client to list of connected clients
            clientList.Add(client);
            // Create Thread to process client connection
            Thread processClientThread = new Thread(new ParameterizedThreadStart(processClient));
            processClientThread.Start(client);
        }
    }



    // Delegate Method
    void processClient(object data)
    {
        int recv = 0;
        byte[] buffer = new byte[1024];
        if (data is TcpClient)
        {
            using (NetworkStream stream = ((TcpClient)data).GetStream())
            {
                // Read until no more to read.
                while ((recv = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string recievedMsg = Encoding.ASCII.GetString(buffer);
                    System.Diagnostics.Debug.WriteLine("Debug: Recieved Message.\n" + recievedMsg);
                }
            }
        }

    }

    // Start is called before the first frame update
    // void Start()
    // {

    // }

    // Update is called once per frame
    void Update()
    {

    }
}
