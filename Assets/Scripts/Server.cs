using System;
using System.Collections;
using System.Collections.Generic;
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
        // Instantiate clientList
        clientList = new List<TcpClient>();
        // Create Server Endpoint
        endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        // Listens for Client Connections right away.
        Thread tcpListenerThread = new Thread(new ThreadStart(ListenForClientConnections));
        // Sets ListenerThread as background thread.
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
        Debug.Log("Initializing Server");
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
        byte[] headerBuffer = new byte[PacketBuilder.Constants.PACKETHEADERLENGTH];
        if (data is TcpClient)
        {
            using (NetworkStream stream = ((TcpClient)data).GetStream())
            {
                // Read until no more to read.
                while ((recv = stream.Read(headerBuffer, 0, headerBuffer.Length)) != 0)
                {
                    PacketBuilder.ContentTypeEnum type = (PacketBuilder.ContentTypeEnum) headerBuffer[0];
                    int contentLength = BitConverter.ToInt32(headerBuffer, 1);
                    Debug.LogFormat("Debug: Type {0}, ContentLength {1}\n", type, contentLength);
                    byte[] buffer = new byte[contentLength];
                    stream.Read(buffer, 0, contentLength);
                    Debug.LogFormat("Debug: {0}\n", Encoding.ASCII.GetString(buffer));
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
