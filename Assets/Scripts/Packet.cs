using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Packet
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

    public byte[] addPacketHeader(byte[] payload, ContentTypeEnum contentTypeEnum)
    {
        payload[0] = (byte) contentTypeEnum;
        return payload;
    }

    public void buildPacket()
    {

    }

}
