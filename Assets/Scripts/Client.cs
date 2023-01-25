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

    public void initClient(string IP, int port)
    {
        // Create Server Endpoint
        endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        clientConnection = new TcpClient();
        clientConnection.Connect(endPoint);
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
                    Debug.Log("Recieved Message:\n" + recievedMessage);
                }
            }
        }
    }

    public void Send(NetworkStream stream, byte[] payload)
    {
        // Use Write() to send some payload to peer.
        stream.Write(payload, 0, payload.Length);
    }

    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
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
        UnityEngine.Debug.Log("Header: " + BitConverter.ToString(payload,0, PacketBuilder.Constants.PACKETHEADERLENGTH));
        string payloadString = Encoding.ASCII.GetString(payload, PacketBuilder.Constants.PACKETHEADERLENGTH, payload.Length - PacketBuilder.Constants.PACKETHEADERLENGTH);
        Debug.Log("Payload: " + payloadString);
        //UnityEngine.Debug.Log("Payload: " + Encoding.ASCII.GetString(payload, PacketBuilder.Constants.HEADERSIZE, PacketBuilder.Constants.HEADERSIZE).Trim('*'));
        //UnityEngine.Debug.Log("Payload: " + Encoding.ASCII.GetString(payload, 2*PacketBuilder.Constants.HEADERSIZE, PacketBuilder.Constants.HEADERSIZE).Trim('*'));

        NetworkStream stream = clientConnection.GetStream();
        
        stream.Write(payload, 0, payload.Length);
        
        
    }
}
