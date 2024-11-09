using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ConnectionTest1
{
    public ConnectionTest1(int value, string message)
    {
        _value = value;
        _message = message;
    }

    public int _value;
    public string _message;
}
public class ConnectionTest2
{
    public ConnectionTest2(string name, Vector3 pos)
    {
        _name = name;
        _position = pos;
    }

    public string _name;
    public Vector3 _position;
}

public struct SceneLoadedData
{
    //To fill later
}

public static class PacketHandler
{
    public enum PacketType
    {
        None,
        Test1,
        Test2,
        PlayerData,
        SceneLoadedData,
        EntityPlayer,
        EntityEnemy
    }

    static Dictionary<Type, PacketType> encodeTypes = new Dictionary<Type, PacketType>()
    {
        {typeof(object),            PacketType.None}, // Acts as the "null" equivalent
        {typeof(SceneLoadedData),   PacketType.SceneLoadedData},
        {typeof(ConnectionTest1),   PacketType.Test1}, //Change later
        {typeof(ConnectionTest2),   PacketType.Test2}, //Change later
        {typeof(PlayerData),        PacketType.PlayerData}
    };
    static Dictionary<PacketType, Type> decodeTypes = new Dictionary<PacketType, Type>()
    {
        {PacketType.None,           typeof(object)}, // Acts as the "null" equivalent
        {PacketType.SceneLoadedData,typeof(SceneLoadedData)}, 
        {PacketType.Test1,          typeof(ConnectionTest1)}, //Change later
        {PacketType.Test2,          typeof(ConnectionTest2)}, //Change later
        {PacketType.PlayerData,     typeof(PlayerData)}
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

        if (data.Length > 1023) { Debug.LogWarning("Packet data is bigger than max size"); }

        byte[] result = new byte[1024];
        result[0] = (byte)type;

        Buffer.BlockCopy(data, 0, result, 1, data.Length);
        return result;
    }

    public static object DecodeData(byte[] data)
    {
        PacketType type = (PacketType)data[0];

        //Find where in the packet the data ends
        int dataLength = Array.IndexOf(data, (byte)0, 1) - 1;
        if (dataLength < 0) dataLength = data.Length - 1;

        MemoryStream stream = new MemoryStream(data, 1, dataLength);
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
