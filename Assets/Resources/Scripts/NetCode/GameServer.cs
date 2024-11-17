using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using CustomExtensions;

public class GameServer : MonoBehaviour
{
    public static GameServer Singleton { get; private set; }

    Socket serverSocket;

    Dictionary<EndPoint, string> connectedUsers = new Dictionary<EndPoint, string>();
    public Dictionary<uint, object> netObjectsInfo = new Dictionary<uint, object>();

    Dictionary<PacketType, Action<object, EndPoint>> functionsDictionary;

    private void Awake()
    {
        #region Singleton

        if (Singleton != null && Singleton != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Singleton = this;
        }

        DontDestroyOnLoad(this.gameObject);

        #endregion

        StartServerFunctions();
    }

    public void Init()
    {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

        serverSocket.Bind(ipep);

        //Start a new thread to receive messages.
        Thread mainThread = new Thread(Receive);
        mainThread.Start();
    }

    void Receive()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        byte[] data = new byte[1024];
        int recv;

        while(true)
        {
            recv = serverSocket.ReceiveFrom(data, ref Remote);

            if (recv == 0) { continue; }

            (PacketType, object) decodedClass = PacketHandler.DecodeData(data);

            functionsDictionary[decodedClass.Item1](decodedClass.Item2, Remote);
        }
    }

    void BroadCastPacket<T>(PacketType type, T packetToSend, EndPoint sender)
    {
        foreach (var user in connectedUsers)
        {
            if(user.Key.GetIP().ToString() == sender.GetIP().ToString() &&
               user.Key.GetPort() == sender.GetPort()) { continue; }

            IPEndPoint ipep = new IPEndPoint(user.Key.GetIP(), user.Key.GetPort());
            PacketHandler.SendPacket(serverSocket, ipep, type, packetToSend);
        }
    }
    void BroadCastPacket(byte[] packetToSend, EndPoint sender)
    {
        foreach (var user in connectedUsers)
        {
            if (user.Key.GetIP().ToString() == sender.GetIP().ToString() &&
               user.Key.GetPort() == sender.GetPort()) { continue; }

            IPEndPoint ipep = new IPEndPoint(user.Key.GetIP(), user.Key.GetPort());
            PacketHandler.SendPacket(serverSocket, ipep, packetToSend);
        }
    }

    void StartServerFunctions()
    {
        functionsDictionary = new Dictionary<PacketType, Action<object, EndPoint>>()
        {
            { PacketType.PlayerData, (obj, ep) => { AddUserToDictionary(ep, (PlayerData)obj); } },
            { PacketType.SceneLoadedFlag, (obj, ep) => { HandleClientSceneLoaded(ep); } },
            { PacketType.playerActionsList, (obj, ep) => { HandlePlayerActions((Wrappers.PlayerActionList)obj, ep); } },
        };
    }

    void AddUserToDictionary(EndPoint ep, PlayerData playerData)
    {
        connectedUsers.Add(ep, playerData.Username);
    }

    public uint GenerateRandomID()
    {
        byte[] buffer = new byte[4];
        System.Random random = new System.Random();
        uint id;

        do
        {
            random.NextBytes(buffer);
            id = BitConverter.ToUInt32(buffer, 0);

        } while (netObjectsInfo.ContainsKey(id));

        // Convert the byte array to a uint
        return id;
    }

    void HandleClientSceneLoaded(EndPoint ep)
    {
        uint newObjectID = GenerateRandomID();

        netObjectsInfo.Add(newObjectID, new Wrappers.Player(newObjectID, connectedUsers[ep]));

        IPEndPoint ipep = new IPEndPoint(ep.GetIP(), ep.GetPort());

        byte[] encodedDictionary = PacketHandler.EncodeDictionary(netObjectsInfo);
        PacketHandler.SendPacket(serverSocket, ipep, encodedDictionary);
        BroadCastPacket(encodedDictionary, ep);
    }

    void HandlePlayerActions(Wrappers.PlayerActionList list, EndPoint senderEP)
    {
        BroadCastPacket(PacketType.playerActionsList, list, senderEP);
    }
}
