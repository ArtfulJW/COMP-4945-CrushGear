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
        public static int HEADERSIZE = 64;
        public static string PLAYERFORMAT = "{0},{1},{2},{3}";
        public static string PLAYERDISCONNECTFORMAT = "{0},{1}";
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

    public void addPlayerDisconnectBodyPart(MemoryStream memoryStream, int id)
    {
        // Init StringBuilder
        StringBuilder stringBuilder = new StringBuilder();

        // Build payload
        stringBuilder.AppendFormat(Constants.PLAYERDISCONNECTFORMAT, ContentTypeEnum.PlayerDisconnect, id.ToString());

        // Pack payload and write to memoryStream
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(stringBuilder.ToString())), 0, Constants.HEADERSIZE);
    }

    public void addPlayerBodyPart(MemoryStream memoryStream, int id, float xcoord, float ycoord)
    {
        // Init StringBuilder
        StringBuilder stringBuilder = new StringBuilder();

        // Build payload
        stringBuilder.AppendFormat(Constants.PLAYERFORMAT, ContentTypeEnum.Player.ToString(), id.ToString(), xcoord.ToString(), ycoord.ToString());

        // Pack payload and write to memoryStream
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(stringBuilder.ToString())), 0, Constants.HEADERSIZE);
        //UnityEngine.Debug.Log(Encoding.ASCII.GetString(memoryStream.ToArray()));
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
        // Get GameState Singleton
        GameManager gameManager = (GameManager)GameObject.Find("GameManager").GetComponent("GameManager");
        
        // Test Player - Later on pull player details directly from GameManager Singleton
        Player p = new Player();

        // Init MemoryStream
        using MemoryStream memoryStream = new MemoryStream();
        UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

        // Write Header information from 0th index to Constants.HEADERSIZE index
        foreach (ContentTypeEnum contentType in container)
        {
            switch (contentType)
            {
                case ContentTypeEnum.Player:
                    addPlayerBodyPart(memoryStream, p.id, p.xcoord, p.ycoord);
                    break;
                case ContentTypeEnum.PlayerConnect:
                    // Build this BodyPart
                    break;
                case ContentTypeEnum.PlayerDisconnect:
                    addPlayerDisconnectBodyPart(memoryStream, p.id);
                    break;
                default:
                    break;
            }
        }

        //UnityEngine.Debug.Log("HELL: " + Encoding.ASCII.GetString(memoryStream.ToArray()));
        return memoryStream.ToArray();
    }

}
