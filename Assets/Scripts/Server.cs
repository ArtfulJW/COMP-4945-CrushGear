using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour {
    private IPEndPoint endPoint = null;
    private List<TcpClient> clientList = null;

    private Queue<PacketBuilder.Packet> packetQueue = new Queue<PacketBuilder.Packet> ();
    private object queueLock = new System.Object();

    public void initServer(string IP, int port) {
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
    void ListenForClientConnections() {
        TcpListener tcpListener = new TcpListener(endPoint);
        tcpListener.Start();

        // TODO: Add exception handling
        while (true) {
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
    void processClient(object client) {
        int recv = 0;
        byte[] headerBuffer = new byte[PacketBuilder.Constants.PACKETHEADERLENGTH];
        if (client is not TcpClient) {
            return;
        }
        using (NetworkStream stream = ((TcpClient) client).GetStream()) {
            // Read until no more to read.
            while ((recv = stream.Read(headerBuffer, 0, headerBuffer.Length)) != 0) {
                PacketBuilder.ContentTypeEnum type = (PacketBuilder.ContentTypeEnum) headerBuffer[0];
                int contentLength = BitConverter.ToInt32(headerBuffer, 1);
                //Debug.LogFormat("Debug: Type {0}, ContentLength {1}\n", type, contentLength);
                byte[] data = new byte[contentLength];
                stream.Read(data, 0, contentLength);
                //Debug.LogFormat("Debug: {0}\n", Encoding.ASCII.GetString(buffer));
                enqueuePacket(type, data);
            }
        }

    }

    void enqueuePacket(PacketBuilder.ContentTypeEnum type, byte[] data) {
        lock (queueLock) {
            packetQueue.Enqueue(new PacketBuilder.Packet(type, data));
        }
    }

    void Send(byte[] payload) {
        foreach (TcpClient client in clientList) {
            NetworkStream stream = client.GetStream();
            stream.Write(payload, 0, payload.Length);
        }
    }

    // Start is called before the first frame update
    // void Start()
    // {

    // }

    // Update is called once per frame
    void Update() {
        Queue<PacketBuilder.Packet> tempQ;
        lock(queueLock) {
            tempQ = packetQueue;
            packetQueue = new Queue<PacketBuilder.Packet>();
        }
        Debug.LogFormat("Debug: QueueLength: {0}", tempQ.Count);
        foreach (PacketBuilder.Packet packet in tempQ) {
            Debug.LogFormat("Debug: Packet: {0} {1}\n", packet.contentType, Encoding.ASCII.GetString(packet.data));
        }
    }
}
