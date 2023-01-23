using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class PacketBuilder
{
    public static class Constants
    {
        public static int HEADERSIZE = 16;
    }


    public enum ContentTypeEnum : byte
    {
        Player,
        PlayerConnect,
        PlayerDisconnect
    }

    public void addPacketHeader(ContentTypeEnum contentTypeEnum)
    { 
        
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

    public byte[] buildPacket(ContentTypeEnum contentTypeEnum)
    {
        // Init MemoryStream
        using MemoryStream memoryStream = new MemoryStream();
        // Set Payload buffer Size
        //memoryStream.SetLength(1000);

        UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

        // Write Header information from 0th index to Constants.HEADERSIZE index
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(contentTypeEnum.ToString())), 0, Constants.HEADERSIZE);
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(ContentTypeEnum.PlayerConnect.ToString())), 0, Constants.HEADERSIZE);
        memoryStream.Write(Encoding.ASCII.GetBytes(packString(ContentTypeEnum.PlayerDisconnect.ToString())), 0, Constants.HEADERSIZE);

        //string plyr = prepString(ContentTypeEnum.PlayerConnect.ToString());
        //UnityEngine.Debug.Log(plyr);    


        //UnityEngine.Debug.Log("HELL: " + Encoding.ASCII.GetString(memoryStream.ToArray()));
        return memoryStream.ToArray();
    }

}
