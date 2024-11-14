using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public struct SceneLoadedData
{
    //To fill later
}

public enum PacketType
{
    None,
    PlayerData,
    AssignOwnership,
    SceneLoadedFlag,
    netObjsDictionary,
}

public static class PacketHandler
{
    static Dictionary<PacketType, Type> decodeTypes = new Dictionary<PacketType, Type>()
    {
        {PacketType.None,           typeof(object)}, // Acts as the "null" equivalent
        {PacketType.SceneLoadedFlag,typeof(SceneLoadedData)},
        {PacketType.PlayerData,     typeof(PlayerData)},
        {PacketType.AssignOwnership,typeof(uint)}
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

    public static (PacketType, object) DecodeData(byte[] data)
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

        if (type == PacketType.netObjsDictionary) { return (type, JsonToDictionary(json)); }

        return (type, JsonUtility.FromJson(json, decodeTypes[type]));
    }

    public static void SendPacket<T>(Socket senderSocket, IPEndPoint targetEndPoint, PacketType type, T classInstance)
    {
        byte[] data = EncodeData(type, classInstance);
        senderSocket.SendTo(data, data.Length, SocketFlags.None, targetEndPoint);
    }
    public static void SendPacket(Socket senderSocket, IPEndPoint targetEndPoint, byte[] data)
    {
        senderSocket.SendTo(data, data.Length, SocketFlags.None, targetEndPoint);
    }

    [Serializable]
    public class DictionaryEntryWrapper
    {
        public uint _key;
        public string _valueType;
        public string _valueJSON;

        public DictionaryEntryWrapper(uint key, string valueType, string valueJSON)
        {
            _key = key;
            _valueType = valueType;
            _valueJSON = valueJSON;
        }
    }
    [Serializable]
    public class DictionaryListWrapper
    {
        public List<DictionaryEntryWrapper> _list;

        public DictionaryListWrapper(List<DictionaryEntryWrapper> list)
        {
            _list = list;
        }
    }

    public static byte[] EncodeDictionary(Dictionary<uint, object> dictionaryToEncode)
    {
        List<DictionaryEntryWrapper> dictionaryList = new List<DictionaryEntryWrapper>();

        foreach (var item in dictionaryToEncode)
        {
            dictionaryList.Add(new DictionaryEntryWrapper(item.Key, item.Value.GetType().Name, JsonUtility.ToJson(item.Value)));
        }

        DictionaryListWrapper wrapper = new DictionaryListWrapper(dictionaryList);
        string json = JsonUtility.ToJson(wrapper);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);

        byte[] data = stream.ToArray();

        writer.Close();
        stream.Close();

        if (data.Length > 1023) { Debug.LogWarning("Packet data is bigger than max size"); }

        byte[] result = new byte[1024];
        result[0] = (byte)PacketType.netObjsDictionary;

        Buffer.BlockCopy(data, 0, result, 1, data.Length);
        return result;
    }
    public static Dictionary<uint, object> JsonToDictionary(string json)
    {
        DictionaryListWrapper wrapper = JsonUtility.FromJson<DictionaryListWrapper>(json);

        var dictionary = new Dictionary<uint, object>();

        foreach (var entry in wrapper._list)
        {
            Type type = Type.GetType(entry._valueType);

            object obj = JsonUtility.FromJson(entry._valueJSON, type);

            //dictionary[entry._key] = value; //Works as well
            dictionary.Add(entry._key, obj);
        }

        return dictionary;
    }
}
