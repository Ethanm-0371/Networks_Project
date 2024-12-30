using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public enum PacketType
{
    None,
    Ping,
    PlayerData,
    AssignOwnership,
    SceneLoadedFlag,
    netObjsDictionary,
    playerActionsList,
}

public static class PacketHandler
{
    private static byte[] EncodePacket(PacketType type, NetInfo objectToAdd)
    {
        byte[] serializedClass = objectToAdd.Serialize();

        byte[] result = new byte[serializedClass.Length + 3];                   //+3 because 1 from the PaketType byte, 1 from the list flag and 1 from the type of data
        result[0] = (byte)type;
        result[1] = 0;                                                          //Indicates that the packet is not a list
        result[2] = (byte)Wrappers.Types.encodeTypes[objectToAdd.GetType()];    //Use another byte to specify the type of the data;

        Buffer.BlockCopy(serializedClass, 0, result, 3, serializedClass.Length);

        return result;
    }
    private static byte[] EncodePacket(PacketType type, List<NetInfo> objectsToAdd)
    {
        List<byte[]> dataToAdd = new List<byte[]>();
        int totalDataSize = 0;

        foreach (var item in objectsToAdd)                                      //First we make a list of all of the objects we want to encode and their respective info
        {
            byte[] entry = item.Serialize();
            dataToAdd.Add(BitConverter.GetBytes((short)entry.Length));          //Put 2 bytes in front of the data to specify the size of it

            byte[] classType = new byte[1];
            classType[0] = (byte)Wrappers.Types.encodeTypes[item.GetType()];    //Use another byte to specify the type of the data
            dataToAdd.Add(classType);

            dataToAdd.Add(entry);
            totalDataSize += entry.Length + 3;                                  //+3 from the 2 bytes of the size and the 1 from the type

            if (totalDataSize > 1023)
                Debug.LogWarning("Packet data is bigger than 1024 bytes");
        }

        byte[] result = new byte[totalDataSize + 3];                            //+3 because 1 from the PaketType byte, 1 from the list flag and 1 from number of entries
        result[0] = (byte)type;
        result[1] = 1;                                                          //Indicates that the packet IS a list
        result[2] = (byte)(dataToAdd.Count / 3);                                //Divide three because each entry is size + type + data

        int currentOffset = 3;
        foreach (var entry in dataToAdd)
        {
            Buffer.BlockCopy(entry, 0, result, currentOffset, entry.Length);
            currentOffset += entry.Length;
        }

        return result;
    }

    public static (PacketType, NetInfo) DecodeSinglePacket(byte[] packet)
    {
        PacketType type = (PacketType)packet[0];

        Type dataType = Wrappers.Types.decodeTypes[(Wrappers.Types.ClassIdentifyers)packet[2]]; //Read third byte of the packet because second is List flag

        byte[] classToDecode = new byte[packet.Length - 3];                                     //-3 due to the one from PacketType, the one from the list flag and the one from the data type
        Buffer.BlockCopy(packet, 3, classToDecode, 0, packet.Length - 3);                       //Copy the actual class info from the original array

        NetInfo decodedClass = (NetInfo)Activator.CreateInstance(dataType);
        decodedClass.Deserialize(classToDecode);

        return (type, decodedClass);
    }
    public static (PacketType, List<NetInfo>) DecodeMultiPacket(byte[] packet)
    {
        List<NetInfo> returnList = new List<NetInfo>();

        PacketType type = (PacketType)packet[0];
        int numberOfEntries = packet[2];

        int offset = 3;

        for (int i = 0; i < numberOfEntries; i++)
        {
            int objectSize = BitConverter.ToInt16(packet, offset);                                          //Read first byte of the entry to get the size of it
            offset += 2;

            Type dataType = Wrappers.Types.decodeTypes[(Wrappers.Types.ClassIdentifyers)packet[offset]];    //Read second byte of the entry to get its type
            offset += 1;

            byte[] classToDecode = new byte[objectSize];
            Buffer.BlockCopy(packet, offset, classToDecode, 0, objectSize);                                 //Copy the class info from the original array to process it
            offset += objectSize;

            NetInfo decodedClass = (NetInfo)Activator.CreateInstance(dataType);
            decodedClass.Deserialize(classToDecode);

            returnList.Add(decodedClass);
        }

        return (type, returnList);
    }

    public static void SendPacket(Socket senderSocket, IPEndPoint targetEndPoint, PacketType type, NetInfo infoToSend)
    {
        byte[] data = EncodePacket(type, infoToSend);
        senderSocket.SendTo(data, data.Length, SocketFlags.None, targetEndPoint);
    }
    public static void SendPacket(Socket senderSocket, IPEndPoint targetEndPoint, PacketType type, List<NetInfo> infoToSend)
    {
        byte[] data = EncodePacket(type, infoToSend);
        senderSocket.SendTo(data, data.Length, SocketFlags.None, targetEndPoint);
    }
}
