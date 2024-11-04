using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum MessageType
{
    Connect,
    Disconnect,
    Message,
    None
}

public class MessageData
{
    public MessageData(MessageType type, string message = "")
    {
        _type = type;
        _message = message;
    }

    public MessageType _type;
    public string _message;
}

public static class ChatUtilities
{
    public static byte[] EncodeData(MessageData message)
    {
        string json = JsonUtility.ToJson(message);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);

        byte[] data = stream.ToArray();

        writer.Close();
        stream.Close();

        return data;
    }
    public static MessageData DecodeData(byte[] data)
    {
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();
        return JsonUtility.FromJson<MessageData>(json);
    }

    public static void SendMessageUDP(Socket senderSocket, IPEndPoint targetEndPoint, MessageType type, string message = "")
    {
        byte[] data = EncodeData(new MessageData(type, message));

        senderSocket.SendTo(data, data.Length, SocketFlags.None, targetEndPoint);
    }
    public static void SendMessageTCP(Socket socketToSendTo, MessageType type, string message = "")
    {
        byte[] data = EncodeData(new MessageData(type, message));

        socketToSendTo.Send(data);
    }

    public static void AddMessageToTextBox(TextMeshProUGUI textBox, string messageToAdd)
    {
        textBox.text = $"{textBox.text}\n{messageToAdd}";
    }

    //In case the message has to be printed from the main thread.
    public static void AddMessageToQueue(List<(TextMeshProUGUI, string)> queue, ref bool boolToSet, TextMeshProUGUI textBox, string messageToAdd)
    {
        queue.Add((textBox, messageToAdd));
        boolToSet = false;
    }
}
