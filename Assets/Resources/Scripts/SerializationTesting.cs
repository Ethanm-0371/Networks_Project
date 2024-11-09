using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

enum PacketType
{
    Message,
    Entity,
    None
}

public class TestingMessage
{
    public TestingMessage(int value, string message)
    {
        _value = value;
        _message = message;
    }

    public int _value;
    public string _message;
}
public class TestingEntity
{
    public TestingEntity(string name, Vector3 pos)
    {
        _name = name;
        _position = pos;
    }

    public string _name;
    public Vector3 _position;
}

public class SerializationTesting : MonoBehaviour
{
    Dictionary<PacketType, Type> typesDictionary = new Dictionary<PacketType, Type>()
    {
        {PacketType.Message, typeof(TestingMessage)},
        {PacketType.Entity, typeof(TestingEntity)},
        {PacketType.None, null}
    };


    TestingMessage message = new TestingMessage(4, "Hola q tal");
    TestingEntity entity = new TestingEntity("Goku Calvo", new Vector3(12, 7, 3.5f));

    void Start()
    {
        //byte[] encoded1 = EncodeClassEnum<TestingMessage>(PacketType.Message, message);
        //byte[] encoded2 = EncodeClassEnum<TestingEntity>(PacketType.Entity, entity);

        //PrintEncoded(encoded1);
        //PrintEncoded(encoded2);

        //var decodedClass1 = DecodeClassEnum(encoded1);
        //var decodedClass2 = DecodeClassEnum(encoded2);

        //ChooseBehaviour(decodedClass1);
        //ChooseBehaviour(decodedClass2);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

        Debug.Log(endPoint.Address.ToString());
        Debug.Log(endPoint.Port);
    }

    byte[] EncodeClassEnum<T>(PacketType type, T classToEncode)
    {
        string json = JsonUtility.ToJson(classToEncode);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);

        byte[] data = stream.ToArray();

        writer.Close();
        stream.Close();

        //---------------------------------------------------

        Array.Resize(ref data, data.Length + 1);

        data[^1] = (byte)type; // data[data.Length - 1] would also work

        return data;
    }

    object DecodeClassEnum(byte[] data)
    {
        PacketType type = (PacketType)data[^1]; // Read the identifier from the last byte

        MemoryStream stream = new MemoryStream(data, 0, data.Length - 1);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();

        reader.Close();
        stream.Close();

        return JsonUtility.FromJson(json, typesDictionary[type]);
    }

    void ChooseBehaviour(object decodedClass)
    {
        switch (decodedClass)
        {
            case TestingMessage message:
                Debug.Log($"Message is: {message._message} and value is: {message._value}");
                break;
            case TestingEntity entity:
                Debug.Log($"Entity name is: {entity._name} and its position is: {entity._position.x},{entity._position.y},{entity._position.z}");
                break;
            default:
                Debug.LogError("Could not find decoded class");
                break;
        }
    }

    void PrintEncoded(byte[] data)
    {
        int i = 0;
        string byteToString = "";

        foreach (byte byteInfo in data)
        {
            byteToString += data[i].ToString();
            byteToString += " ";
            i++;
        }

        Debug.Log(byteToString);
    }
}
