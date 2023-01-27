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
    private static Dictionary<string,TcpClient> clientMap = null;
    private PacketBuilder packetBuilder = null;

    private Queue<PacketBuilder.Packet> packetQueue = new Queue<PacketBuilder.Packet>();
    private object packetLock = new System.Object();

    private Queue<TcpClient> newClientQueue = new Queue<TcpClient>();
    private object newClientLock = new System.Object();


    public void initServer(string IP, int port)
    {
        // Instantiate clientList
        clientMap = new Dictionary<string, TcpClient>();
        // Create Server Endpoint
        endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        // Listens for Client Connections right away.
        Thread tcpListenerThread = new Thread(new ThreadStart(ListenForClientConnections));
        // Sets ListenerThread as background thread.
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();

        packetBuilder = new PacketBuilder();
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
            //Enqueue connection for processing
            enqueueClient(client);
            
        }
    }

    void enqueueClient(TcpClient client)
    {
        lock(newClientLock)
        {
            newClientQueue.Enqueue(client);
        }
    }


    // Delegate Method
    void receiveClient(object client)
    {
        if (client is not TcpClient)
        {
            return;
        }
        using (NetworkStream stream = ((TcpClient) client).GetStream())
        {
            int recv = 0;
            byte[] headerBuffer = new byte[PacketBuilder.Constants.PACKETHEADERLENGTH];
            // Read until no more to read.
            while ((recv = stream.Read(headerBuffer, 0, headerBuffer.Length)) != 0)
            {
                //TODO ERROR correction
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

    void enqueuePacket(PacketBuilder.ContentTypeEnum type, byte[] data)
    {
        lock (packetLock)
        {
            packetQueue.Enqueue(new PacketBuilder.Packet(type, data));
        }
    }

    /// <summary>
    /// Send payload to every client in the clientMap
    /// </summary>
    /// <param name="payload"></param>
    public static void Send(byte[] payload)
    {
        foreach (KeyValuePair<string, TcpClient> client in clientMap)
        {
            NetworkStream stream = client.Value.GetStream();
            stream.Write(payload, 0, payload.Length);
        }
    }

    /// <summary>
    /// Send payload to client specified by id
    /// </summary>
    /// <param name="payload">Packet to send to client</param>
    /// <param name="id"></param>
    public static void Send(byte[] payload, string id)
    {
        TcpClient client = null;
        if(clientMap.TryGetValue(id, out client))
        {
            NetworkStream stream = client.GetStream();
            stream.Write(payload, 0, payload.Length);
        }
    }

    // Update is called once per frame
    void Update()
    {
        processClient();
        processPackets();
        PacketBuilder packetBuilder = new PacketBuilder();
        byte[] payload = packetBuilder.buildPacket(PacketBuilder.ContentTypeEnum.GameState);
        Send(payload);
    }

    void processClient()
    {
        Queue<TcpClient> tempQ;
        lock (newClientLock)
        {
            tempQ = newClientQueue;
            newClientQueue = new Queue<TcpClient>();
        }
        foreach (TcpClient client in tempQ)
        {
            // Create Thread to process client connection
            Thread receiveClientThread = new Thread(new ParameterizedThreadStart(receiveClient));
            receiveClientThread.Start(client);
            //  Genterate ID
            string id = generateID();
            //  Map ID and client
            clientMap.Add(id, client);
            //  Give Gamemanager new ID to process
            GameManager.getInstance.processNewClientConnection(id);
        }
    }
    
    //TODO
    string generateID()
    {
        return "NEWID";
    }

    void processPackets()
    {
        Queue<PacketBuilder.Packet> tempQ;
        lock (packetLock)
        {
            tempQ = packetQueue;
            packetQueue = new Queue<PacketBuilder.Packet>();
        }
        //Debug.LogFormat("Debug: QueueLength: {0}", tempQ.Count);
        foreach (PacketBuilder.Packet packet in tempQ)
        {
            //TODO PROCESS PACKETS
            Debug.LogFormat("Debug: Packet: {0} {1}\n", packet.contentType, Encoding.ASCII.GetString(packet.data));
        }
    }
}
