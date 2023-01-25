using System;
using System.Collections;
using System.Collections.Generic;
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

    private Queue<PacketBuilder.Packet> packetQueue = new Queue<PacketBuilder.Packet>();
    private object queueLock = new System.Object();

    public void initClient(string IP, int port)
    {
        // Create Server Endpoint
        endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        clientConnection = new TcpClient();
        while (!clientConnection.Connected)
        {
            Debug.Log("Connecting");
            clientConnection.Connect(endPoint);
        }

        // Listens for Client Connections right away.
        Thread tcpClientThread = new Thread(new ThreadStart(ConnectionListener));
        // Sets ListenerThread as background thread.
        tcpClientThread.IsBackground = true;
        tcpClientThread.Start();
        Debug.Log("Initializing Client");
    }

    // Delegate Method
    void ConnectionListener()
    {
        // TODO: Add exception handling
        while (true)
        {
            using (NetworkStream stream = clientConnection.GetStream())
            {
                int recv = 0;
                byte[] headerBuffer = new byte[PacketBuilder.Constants.PACKETHEADERLENGTH];
                while ((recv = stream.Read(headerBuffer, 0, headerBuffer.Length)) != 0)
                {
                    PacketBuilder.ContentTypeEnum type = (PacketBuilder.ContentTypeEnum) headerBuffer[0];
                    int contentLength = BitConverter.ToInt32(headerBuffer, 1);
                    Debug.LogFormat("Debug: Type {0}, ContentLength {1}\n", type, contentLength);
                    byte[] data = new byte[contentLength];
                    stream.Read(data, 0, contentLength);
                    Debug.LogFormat("Debug: {0}\n", Encoding.ASCII.GetString(data));
                    enqueuePacket(type, data);
                }


            }
            //Connection Closed
            Debug.Log("Stream Closed");
        }
    }



    public void Send(byte[] payload)
    {
        NetworkStream stream = clientConnection.GetStream();
        // Use Write() to send some payload to peer.
        stream.Write(payload, 0, payload.Length);
    }

    // Update is called once per frame
    void Update()
    {
        processPackets();
        // Init PacketBuilder
        PacketBuilder packetBuilder = new PacketBuilder();

        // Build Packet
        byte[] payload;
        payload = packetBuilder.buildPacket(PacketBuilder.ContentTypeEnum.Player);

        //UnityEngine.Debug.Log("Built Payload: " + Encoding.ASCII.GetString(payload));

        // Parsing from 0th index to the length of the header.
        //string[] playerInfo = Encoding.ASCII.GetString(payload).Trim('*').Split(',');

        //foreach(string parameters in playerInfo)
        //{
        //UnityEngine.Debug.Log(payload);
        //}
        //UnityEngine.Debug.Log("Header: " + BitConverter.ToString(payload,0, PacketBuilder.Constants.PACKETHEADERLENGTH));
        //string payloadString = Encoding.ASCII.GetString(payload, PacketBuilder.Constants.PACKETHEADERLENGTH, payload.Length - PacketBuilder.Constants.PACKETHEADERLENGTH);
        //Debug.Log("Payload: " + payloadString);
        //UnityEngine.Debug.Log("Payload: " + Encoding.ASCII.GetString(payload, PacketBuilder.Constants.HEADERSIZE, PacketBuilder.Constants.HEADERSIZE).Trim('*'));
        //UnityEngine.Debug.Log("Payload: " + Encoding.ASCII.GetString(payload, 2*PacketBuilder.Constants.HEADERSIZE, PacketBuilder.Constants.HEADERSIZE).Trim('*'));


        Send(payload);


    }

    void enqueuePacket(PacketBuilder.ContentTypeEnum type, byte[] data)
    {
        lock (queueLock)
        {
            packetQueue.Enqueue(new PacketBuilder.Packet(type, data));
        }
    }

    void processPackets()
    {
        Queue<PacketBuilder.Packet> tempQ;
        lock (queueLock)
        {
            tempQ = packetQueue;
            packetQueue = new Queue<PacketBuilder.Packet>();
        }
        Debug.LogFormat("Debug: QueueLength: {0}", tempQ.Count);
        foreach (PacketBuilder.Packet packet in tempQ)
        {
            Debug.LogFormat("Debug: Packet: {0} {1}\n", packet.contentType, Encoding.ASCII.GetString(packet.data));
        }
    }
}
