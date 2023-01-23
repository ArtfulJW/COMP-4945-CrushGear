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

    public void addPlayerInfo(MemoryStream memoryStream, int id, float xcoord, float ycoord)
    {
        string playerInfo = ContentTypeEnum.Player.ToString() + "," + "id=" + id.ToString() + "," + "xcoord=" + xcoord.ToString() + "," + "ycoord=" + ycoord.ToString();
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(playerInfo)), 0, Constants.HEADERSIZE);
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
                    addPlayerInfo(memoryStream, p.id, p.xcoord, p.ycoord);
                    break;
                case ContentTypeEnum.PlayerConnect:
                    // Build this BodyPart
                    break;
                case ContentTypeEnum.PlayerDisconnect:
                    // Build this BodyPart
                    break;
                default:
                    break;
            }
        }

        //UnityEngine.Debug.Log("HELL: " + Encoding.ASCII.GetString(memoryStream.ToArray()));
        return memoryStream.ToArray();
    }

}
