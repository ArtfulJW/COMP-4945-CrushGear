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
    public struct Packet {
        public ContentTypeEnum contentType;
        public byte[] data;

        public Packet(ContentTypeEnum contentType, byte[] data) {
            this.contentType = contentType;
            this.data = data;  
        }
    }
    public static class Constants
    {
        // Tentative Formats for each Packet.
        public static string PLAYER_FORMAT = "{0} {1} {2}";
        public static string PLAYERASSIGNMENT_FORMAT = "{0}";
        public static string PLAYERDISCONNECT_FORMAT = "{0}";
        public static string GAMESTATE_FORMAT = "{0}";
        // Of order ContentTypeEnum, Packet Length 
        public static string PACKETHEADER = "{0}{1}";
        // Size of ContentTypeEnum, Packet Length data types
        public static byte PACKETHEADERLENGTH = 1 + 4;
        //public static int HEADERSIZE = 64;
        public const string DELIM = ",";
    }


    public enum ContentTypeEnum : byte
    {
        Player,
        PlayerIdAssignment,
        PlayerDisconnect,
        GameState
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
    private void buildPlayerIdAssignmentPacket(MemoryStream memoryStream)
    {
        // Get GameState Singleton
        GameManager gameManager = (GameManager)GameObject.Find("GameManager").GetComponent<GameManager>();

        // Init StringBuilder
        StringBuilder payloadBuilder = new StringBuilder();

        // Build payload
        payloadBuilder.AppendFormat(Constants.PLAYERASSIGNMENT_FORMAT, gameManager.newConnectionID);
        // Build header
        buildPacketHeader(memoryStream, ContentTypeEnum.PlayerIdAssignment, payloadBuilder.Length);

        memoryStream.Write(Encoding.ASCII.GetBytes(payloadBuilder.ToString()));

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
        StringBuilder payloadBuilder = new StringBuilder();

        // Build payload
        payloadBuilder.AppendFormat(Constants.PLAYERDISCONNECT_FORMAT, gameManager.localPlayer.id.ToString());

        buildPacketHeader(memoryStream, ContentTypeEnum.PlayerDisconnect, payloadBuilder.Length);
        // Pack payload and write to memoryStream
        memoryStream.Write(Encoding.ASCII.GetBytes(payloadBuilder.ToString()));
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
        payloadBuilder.AppendFormat(Constants.PLAYER_FORMAT, gameManager.localPlayer.id.ToString(), gameManager.localPlayer.xcoord.ToString(), gameManager.localPlayer.ycoord.ToString());

        // Pack header and write to memoryStream
        buildPacketHeader(memoryStream, ContentTypeEnum.Player, payloadBuilder.Length);

        // Pack payload and write to memoryStream
        memoryStream.Write(Encoding.ASCII.GetBytes(payloadBuilder.ToString()));
        //UnityEngine.Debug.Log(Encoding.ASCII.GetString(memoryStream.ToArray()));
    }

    private void buildGameStatePacket(MemoryStream memoryStream) {
        // Get GameState Singleton
        GameManager gameManager = (GameManager) GameObject.Find("GameManager").GetComponent<GameManager>();

        // Init StringBuilder
        StringBuilder payloadBuilder = new StringBuilder();

        // Build payload
        payloadBuilder.AppendFormat(Constants.GAMESTATE_FORMAT, gameManager.ToString());

        // Pack header and write to memoryStream
        buildPacketHeader(memoryStream, ContentTypeEnum.GameState, payloadBuilder.Length);

        // Pack payload and write to memoryStream
        memoryStream.Write(Encoding.ASCII.GetBytes(payloadBuilder.ToString()));
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
                case ContentTypeEnum.PlayerIdAssignment:
                    buildPlayerIdAssignmentPacket(memoryStream);
                    break;
                case ContentTypeEnum.PlayerDisconnect:
                    buildPlayerDisconnectPacket(memoryStream);
                    break;
                case ContentTypeEnum.GameState:
                    buildGameStatePacket(memoryStream);
                    break;
                default:
                    break;
            }
        }

        //UnityEngine.Debug.Log("HELL: " + Encoding.ASCII.GetString(memoryStream.ToArray()));
        return memoryStream.ToArray();
    }

}
