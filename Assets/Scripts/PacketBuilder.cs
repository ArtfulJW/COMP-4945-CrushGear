using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.XPath;
using Unity.VisualScripting;
using UnityEngine;

public class PacketBuilder
{
    public static class Constants
    {
        // Tentative Formats for each Packet.
        public static string PLAYERFORMAT = "{0} {1} {2}";
        public static string PLAYERCONNECTFORMAT = "{0}{1}";
        public static string PLAYERDISCONNECTFORMAT = "{0}{1}";
        // Of order ContentTypeEnum, Packet Length 
        public static string PACKETHEADER = "{0}{1}";
        // Size of ContentTypeEnum, Packet Length data types
        public static byte PACKETHEADERLENGTH = 4 + 1;
        //public static int HEADERSIZE = 64;
        public const string DELIM = ",";
    }


    public enum ContentTypeEnum : byte
    {
        Player,
        PlayerConnect,
        PlayerDisconnect
    }

    private void buildPacketHeader(MemoryStream memoryStream, ContentTypeEnum contentTypeEnum, int payloadLength)
    {
        // Write Header information from 0th index to Constants.HEADERSIZE index
        byte type = (byte)contentTypeEnum;
        memoryStream.WriteByte(type);
        memoryStream.Write(BitConverter.GetBytes(payloadLength));
       
    }

    /// <summary>
    /// Packet informing server that a client has connected and should be added to the gamestate
    /// <para>
    /// Sent By: Client
    /// </para>
    /// </summary>
    /// <param name="memoryStream"></param>
    private void buildPlayerConnectPacket(MemoryStream memoryStream)
    {
        // Get GameState Singleton
        GameManager gameManager = (GameManager)GameObject.Find("GameManager").GetComponent<GameManager>();

        // Init StringBuilder
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append(ContentTypeEnum.PlayerConnect);

        // Build payload
        foreach (Player player in gameManager.playerList) {
            stringBuilder.Append(Constants.DELIM).AppendFormat(Constants.PLAYERFORMAT, ContentTypeEnum.Player.ToString(), player.id.ToString(), player.xcoord.ToString(), player.ycoord.ToString());
        }

        //TODO
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(stringBuilder.ToString())), 0, Constants.PACKETHEADERLENGTH);

    }

    /// <summary>
    /// Packet informing client/server that a client has disconnected and should be removed from the gamestate
    /// <para>
    /// Sent By: Both
    /// </para>
    /// </summary>
    /// <param name="memoryStream"></param>
    private void buildPlayerDisconnectPacket(MemoryStream memoryStream)
    {
        // Get GameState Singleton
        GameManager gameManager = (GameManager)GameObject.Find("GameManager").GetComponent<GameManager>();

        // Init StringBuilder
        StringBuilder stringBuilder = new StringBuilder();

        // Build payload
        stringBuilder.AppendFormat(Constants.PLAYERDISCONNECTFORMAT, ContentTypeEnum.PlayerDisconnect, gameManager.localPlayer.id.ToString());

        // Pack payload and write to memoryStream
        //TODO
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(stringBuilder.ToString())), 0, Constants.PACKETHEADERLENGTH);
    }

    /// <summary>
    /// Packet informing client/server of a player's current status
    /// <para>
    /// Sent By: Both
    /// </para>
    /// </summary>
    /// <param name="memoryStream"></param>
    private void buildPlayerPacket(MemoryStream memoryStream)
    {
        // Get GameState Singleton
        GameManager gameManager = (GameManager)GameObject.Find("GameManager").GetComponent<GameManager>();

        // Init StringBuilder
        StringBuilder payloadBuilder = new StringBuilder();

        // Build payload
        payloadBuilder.AppendFormat(Constants.PLAYERFORMAT, gameManager.localPlayer.id.ToString(), gameManager.localPlayer.xcoord.ToString(), gameManager.localPlayer.ycoord.ToString());

        // Pack header and write to memoryStream
        buildPacketHeader(memoryStream, ContentTypeEnum.Player, payloadBuilder.Length);

        // Pack payload and write to memoryStream
        memoryStream.Write(Encoding.ASCII.GetBytes(payloadBuilder.ToString()));
        //UnityEngine.Debug.Log(Encoding.ASCII.GetString(memoryStream.ToArray()));
    }

    private string packString(string str)
    {
        //TODO
        while (str.Length < Constants.PACKETHEADERLENGTH) {
            str += '*';
        }
        //UnityEngine.Debug.Log(str);
        return str;

    }

    public byte[] buildPacket(params ContentTypeEnum[] container)
    {

        // Init MemoryStream
        using MemoryStream memoryStream = new MemoryStream();

        // Write Header information from 0th index to Constants.HEADERSIZE index
        foreach (ContentTypeEnum contentType in container) {
            // Pass in MemoryStream to append BodyPart. Convert to byte array for output.
            switch (contentType) {
                case ContentTypeEnum.Player:
                    buildPlayerPacket(memoryStream);
                    break;
                case ContentTypeEnum.PlayerConnect:
                    buildPlayerConnectPacket(memoryStream);
                    break;
                case ContentTypeEnum.PlayerDisconnect:
                    buildPlayerDisconnectPacket(memoryStream);
                    break;
                default:
                    break;
            }
        }

        //UnityEngine.Debug.Log("HELL: " + Encoding.ASCII.GetString(memoryStream.ToArray()));
        return memoryStream.ToArray();
    }

}
