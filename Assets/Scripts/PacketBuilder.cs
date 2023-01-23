using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.XPath;
using UnityEngine;

public class PacketBuilder
{
    public static class Constants
    {
        // Tentative Formats for each Packet.
        public static string PLAYERFORMAT = "{0},{1},{2},{3}";
        public static string PLAYERDISCONNECTFORMAT = "{0},{1}";
        public static int HEADERSIZE = 64;
        public const string DELIM = ",";
    }


    public enum ContentTypeEnum : byte
    {
        Player,
        PlayerConnect,
        PlayerDisconnect
    }

    public void addPacketHeader(MemoryStream memoryStream, ContentTypeEnum contentTypeEnum)
    {

        // Write Header information from 0th index to Constants.HEADERSIZE index
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(contentTypeEnum.ToString())), 0, Constants.HEADERSIZE);

    }

    public void addPlayerDisconnectBodyPart(MemoryStream memoryStream)
    {
        // Get GameState Singleton
        GameManager gameManager = (GameManager)GameObject.Find("GameManager").GetComponent<GameManager>();

        // Init StringBuilder
        StringBuilder stringBuilder = new StringBuilder();

        // Build payload
        stringBuilder.AppendFormat(Constants.PLAYERDISCONNECTFORMAT, ContentTypeEnum.PlayerDisconnect, gameManager.localPlayer.id.ToString());

        // Pack payload and write to memoryStream
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(stringBuilder.ToString())), 0, Constants.HEADERSIZE);
    }

    public void addPlayerBodyPart(MemoryStream memoryStream)
    {
        // Get GameState Singleton
        GameManager gameManager = (GameManager)GameObject.Find("GameManager").GetComponent<GameManager>();

        // Init StringBuilder
        StringBuilder stringBuilder = new StringBuilder();

        // Build payload
        stringBuilder.AppendFormat(Constants.PLAYERFORMAT, ContentTypeEnum.Player.ToString(), gameManager.localPlayer.id.ToString(), gameManager.localPlayer.xcoord.ToString(), gameManager.localPlayer.ycoord.ToString());

        // Pack payload and write to memoryStream
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(stringBuilder.ToString())), 0, Constants.HEADERSIZE);
        //UnityEngine.Debug.Log(Encoding.ASCII.GetString(memoryStream.ToArray()));
    }

    public void addPlayerConnectBodyPart(MemoryStream memoryStream)
    {
        // Get GameState Singleton
        GameManager gameManager = (GameManager)GameObject.Find("GameManager").GetComponent<GameManager>();

        // Init StringBuilder
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append(ContentTypeEnum.PlayerConnect);

        // Build payload
        foreach(Player player in gameManager.playerList)
        {
            stringBuilder.Append(Constants.DELIM).AppendFormat(Constants.PLAYERFORMAT, ContentTypeEnum.Player.ToString(), player.id.ToString(), player.xcoord.ToString(), player.ycoord.ToString());
        }

        memoryStream.Write(Encoding.ASCII.GetBytes(packString(stringBuilder.ToString())), 0, Constants.HEADERSIZE);

    }

    public string packString(string str)
    {
        while (str.Length < Constants.HEADERSIZE)
        {
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
        foreach (ContentTypeEnum contentType in container)
        {
            // Pass in MemoryStream to append BodyPart. Convert to byte array for output.
            switch (contentType)
            {
                case ContentTypeEnum.Player:
                    addPlayerBodyPart(memoryStream);
                    break;
                case ContentTypeEnum.PlayerConnect:
                    addPlayerConnectBodyPart(memoryStream);
                    break;
                case ContentTypeEnum.PlayerDisconnect:
                    addPlayerDisconnectBodyPart(memoryStream) ;
                    break;
                default:
                    break;
            }
        }

        //UnityEngine.Debug.Log("HELL: " + Encoding.ASCII.GetString(memoryStream.ToArray()));
        return memoryStream.ToArray();
    }

}
