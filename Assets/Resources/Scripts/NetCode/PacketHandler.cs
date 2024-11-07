using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public static class PacketHandler
{
    public enum PacketType
    {
        None,
        EntityPlayer,
        EntityEnemy
    }

    static Dictionary<Type, PacketType> encodeTypes = new Dictionary<Type, PacketType>()
    {
        {null, PacketType.None},
        {typeof(int), PacketType.EntityPlayer}, //Change later
        {typeof(int), PacketType.EntityEnemy}
    };
    static Dictionary<PacketType, Type> decodeTypes = new Dictionary<PacketType, Type>()
    {
        {PacketType.None, null},
        {PacketType.EntityPlayer, typeof(int)}, //Change later
        {PacketType.EntityEnemy, typeof(int)}
    };

    private static byte[] EncodeData<T>(PacketType type, T classToEncode)
    {
        string json = JsonUtility.ToJson(classToEncode);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);

        byte[] data = stream.ToArray();

        writer.Close();
        stream.Close();

        Array.Resize(ref data, data.Length + 1);

        data[data.Length - 1] = (byte)type;

        return data;
    }

    public static object DecodeData(byte[] data)
    {
        PacketType type = (PacketType)data[data.Length - 1];

        MemoryStream stream = new MemoryStream(data, 0, data.Length - 1);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();

        reader.Close();
        stream.Close();

        return JsonUtility.FromJson(json, decodeTypes[type]);
    }

    public static void SendPacket<T>(Socket senderSocket, IPEndPoint targetEndPoint, T classInstance)
    {
        byte[] data = EncodeData(encodeTypes[typeof(T)], classInstance);
        senderSocket.SendTo(data, data.Length, SocketFlags.None, targetEndPoint);
    }
}
