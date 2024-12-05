using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Wrappers;

#if (UNITY_EDITOR) 

public class SerializationPlayground : MonoBehaviour
{
    public enum EncodingType
    {
        Json,
        BinaryWriter
    }

    public EncodingType encodingType;
    public PacketType type;
    public int printAmount;

    public byte[] ClassEncoder(int instancesToEncode, object classToEncode)
    {
        switch (type)
        {
            case PacketType.PlayerData:
                return HandlePlayerData(instancesToEncode, (Player)classToEncode);

            case PacketType.AssignOwnership:
                break;
            case PacketType.SceneLoadedFlag:
                break;
            case PacketType.netObjsDictionary:
                break;
            case PacketType.playerActionsList:
                break;
            case PacketType.None:
                Debug.LogWarning("No type selected");
                break;
            default:
                Debug.LogError("Error reading enum type");
                break;
        }

        return new byte[0];
    }

    //Equivalent to PacketHandler's
    private static byte[] EncodeJson<T>(PacketType type, T classToEncode)
    {
        string json = JsonUtility.ToJson(classToEncode);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);

        byte[] data = stream.ToArray();

        writer.Close();
        stream.Close();

        if (data.Length > 1023) { Debug.LogWarning("Packet data is bigger than max size"); }

        byte[] result = new byte[data.Length + 1];
        result[0] = (byte)type;

        Buffer.BlockCopy(data, 0, result, 1, data.Length);
        return result;
    }

    //Each class should have its own binary encode function.
    //My proposal: implement some kind of "Serializable" interface on NetObject
    //so all of the derived classes have to implement it.
    private static byte[] BinarayEncodePlayer(Player playerToEncode) 
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(playerToEncode.id);
        writer.Write(playerToEncode.o);

        //writer.Write(playerToEncode.position.x);
        //writer.Write(playerToEncode.position.y);
        //writer.Write(playerToEncode.position.z);

        //writer.Write(playerToEncode.rotation.x);
        //writer.Write(playerToEncode.rotation.y);
        //writer.Write(playerToEncode.rotation.z);
        //writer.Write(playerToEncode.rotation.w);

        byte[] data = stream.ToArray();

        stream.Close();
        writer.Close();

        return data;
    }
    private static Player BinarayDecodePlayer(byte[] dataToDecode)
    {
        MemoryStream stream = new MemoryStream(dataToDecode);
        BinaryReader reader = new BinaryReader(stream);

        Player playerToReturn = new Player();

        playerToReturn.id = reader.ReadUInt32();
        playerToReturn.o = reader.ReadString();

        //playerToReturn.position.x = reader.ReadSingle();
        //playerToReturn.position.y = reader.ReadSingle();
        //playerToReturn.position.z = reader.ReadSingle();

        //playerToReturn.rotation.x = reader.ReadSingle();
        //playerToReturn.rotation.y = reader.ReadSingle();
        //playerToReturn.rotation.z = reader.ReadSingle();
        //playerToReturn.rotation.w = reader.ReadSingle();

        stream.Close();

        return playerToReturn;
    }
    private static List<Player> BinaryDecodeMultiplePlayers(byte[] dataToDecode)
    {
        List<Player> playerList = new List<Player>();

        PacketType type = (PacketType)dataToDecode[0];
        int numberOfEntries = dataToDecode[1];

        int offset = 2;

        for (int i = 0; i < numberOfEntries; i++)
        {
            int playerSize = BitConverter.ToInt16(dataToDecode, offset);
            offset += 2;

            byte[] playerToDecode = new byte[playerSize];
            Buffer.BlockCopy(dataToDecode, offset, playerToDecode, 0, playerSize);
            offset += playerSize;

            Player decodedPlayer = BinarayDecodePlayer(playerToDecode);

            playerList.Add(decodedPlayer);
        }

        return playerList;
    }

    private byte[] HandlePlayerData(int instancesToEncode, Player classToEncode)
    {
        switch (encodingType)
        {
            case EncodingType.Json:

                if (instancesToEncode > 1) 
                {
                    List<Player> playerList = new List<Player>();

                    for (int i = 0; i < instancesToEncode; i++)
                    {
                        playerList.Add(classToEncode);
                    }

                    //Json encoding does not exist anymore
                    //return EncodeJson(PacketType.None, new ListWrapper<Player>(playerList));
                    return new byte[0];
                }
                else
                {
                    return EncodeJson(PacketType.None, classToEncode);
                }

            case EncodingType.BinaryWriter:

                byte[] data;

                if (instancesToEncode > 1) //Print more than one
                {
                    List<byte[]> dataToAdd = new List<byte[]>();
                    int totalDataSize = 0;

                    for (int i = 0; i < instancesToEncode; i++)
                    {
                        byte[] entry = BinarayEncodePlayer(classToEncode);
                        dataToAdd.Add(BitConverter.GetBytes((short)entry.Length));  //Put 2 bytes in front of the data to specify the size of the data
                        dataToAdd.Add(entry);
                        totalDataSize += entry.Length + 2; //From the 2 bytes of the size
                    }

                    byte[] result = new byte[totalDataSize + 2]; //1 from the PaketType byte and number of entries
                    result[0] = (byte)type;
                    result[1] = (byte)(dataToAdd.Count/2);

                    int currentOffset = 2;
                    foreach (var entry in dataToAdd)
                    {
                        //PrintByteArray(entry);
                        Buffer.BlockCopy(entry, 0, result, currentOffset, entry.Length);
                        currentOffset += entry.Length;
                    }
                    
                    //DEBUG---------------------------------------------------------------------------------------
                    //List<Player> decodedPlayerList = BinaryDecodeMultiplePlayers(result);

                    //foreach (var item in decodedPlayerList)
                    //{
                    //    Debug.Log($"id: {item.id}, owner: {item.o}, pos: {item.p}, rot: {item.r}");
                    //}
                    //--------------------------------------------------------------------------------------------

                    return result;
                }
                else //Print one
                {
                    data = BinarayEncodePlayer(classToEncode);

                    if (data.Length > 1023) { Debug.LogWarning("Packet data is bigger than 1024 bytes"); }

                    byte[] result = new byte[data.Length + 1];
                    result[0] = (byte)type;

                    Buffer.BlockCopy(data, 0, result, 1, data.Length);

                    //DEBUG---------------------------------------------------------------------------------------
                    //Player decodedPlayer = BinarayDecodePlayer(data);
                    //Debug.Log($"Decoded Player: id:{decodedPlayer.id}, owner: {decodedPlayer.o}, pos: {decodedPlayer.p}, rot: {decodedPlayer.r}");
                    //--------------------------------------------------------------------------------------------

                    return result;
                }

            default:
                Debug.LogError("Error reading encoding type");
                break;
        }

        return new byte[0];
    }

    void PrintByteArray(byte[] toPrint)
    {
        string finalPrint = "";

        foreach (var mybyte in toPrint)
        {
            finalPrint += mybyte.ToString();
            finalPrint += " ";
        }

        Debug.Log(finalPrint);
    }
}

#endif