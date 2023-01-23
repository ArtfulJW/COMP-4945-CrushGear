using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class PacketBuilder
{
    public static class Constants
    {
        public static int HEADERSIZE = 8;
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

    public byte[] buildPacket(byte[] payload, ContentTypeEnum contentTypeEnum)
    {
        // Init MemoryStream
        using MemoryStream memoryStream = new MemoryStream(Constants.HEADERSIZE);

        memoryStream.Write(Encoding.ASCII.GetBytes(contentTypeEnum.ToString()),0, contentTypeEnum.ToString().Length);

        return memoryStream.ToArray();
    }

}
