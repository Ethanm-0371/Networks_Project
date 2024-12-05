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
    public Dictionary<uint, NetInfo> netObjectsInfo = new Dictionary<uint, NetInfo>();

    Queue<(PacketType, object, EndPoint)> functionsQueue = new Queue<(PacketType, object, EndPoint)>();
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

    private void Update()
    {
        if (functionsQueue.Count > 0)
        {
            //Maybe add foreach or while != 0
            (PacketType, object, EndPoint) dequeuedFunction = functionsQueue.Dequeue();
            functionsDictionary[dequeuedFunction.Item1](dequeuedFunction.Item2, dequeuedFunction.Item3);
        }
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

            (PacketType, object) decodedClass;

            //Data[1] defines if the packet contains a list
            if (data[1] != 0) { decodedClass = PacketHandler.NewDecodeMultiPacket(data); }
            else              { decodedClass = PacketHandler.NewDecodeSinglePacket(data); }

            functionsQueue.Enqueue((decodedClass.Item1, decodedClass.Item2, Remote));
            //functionsDictionary[decodedClass.Item1](decodedClass.Item2, Remote);
        }
    }

    void BroadCastPacket(PacketType type, NetInfo objectToSend, EndPoint sender)
    {
        foreach (var user in connectedUsers)
        {
            if (user.Key.GetIP().ToString() == sender.GetIP().ToString() &&
               user.Key.GetPort() == sender.GetPort()) { continue; }

            IPEndPoint ipep = new IPEndPoint(user.Key.GetIP(), user.Key.GetPort());
            PacketHandler.SendPacket(serverSocket, ipep, type, objectToSend);
        }
    }
    void BroadCastPacket(PacketType type, List<NetInfo> infoList, EndPoint sender)
    {
        foreach (var user in connectedUsers)
        {
            if(user.Key.GetIP().ToString() == sender.GetIP().ToString() &&
               user.Key.GetPort() == sender.GetPort()) { continue; }

            IPEndPoint ipep = new IPEndPoint(user.Key.GetIP(), user.Key.GetPort());
            PacketHandler.SendPacket(serverSocket, ipep, type, infoList);
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
            { PacketType.PlayerData, (obj, ep) => { AddUserToDictionary(ep, (Wrappers.UserData)obj); } },
            { PacketType.SceneLoadedFlag, (obj, ep) => { HandleClientSceneLoaded(ep); } },
            { PacketType.playerActionsList, (obj, ep) => { HandlePlayerActions((Wrappers.PlayerActionList)obj, ep); } },
        };
    }

    void AddUserToDictionary(EndPoint ep, Wrappers.UserData playerData)
    {
        connectedUsers.Add(ep, playerData.userName);
    }
    void UpdateNetObjsInfo()
    {
        foreach (var item in GetComponent<NetObjectsHandler>().netGameObjects)
        {
            netObjectsInfo[item.Key] = item.Value.GetComponent<NetObject>().GetNetInfo();
        }
    }
    List<NetInfo> GetNetInfoDictionaryList()
    {
        UpdateNetObjsInfo();

        var netObjDict = GetComponent<NetObjectsHandler>().netGameObjects;

        //Since the whole dictionary must be sent, it can be encoded as a list
        List<NetInfo> objsInfo = new List<NetInfo>();

        foreach (var item in netObjectsInfo)
        {
            Vector3 targetPos = default;
            Quaternion targetRot = default;

            if (netObjDict.TryGetValue(item.Key, out var netObj))
            {
                targetPos = netObj.transform.position;
                targetRot = netObj.transform.rotation;
            }

            objsInfo.Add(new Wrappers.NetObjInfo(item.Key, item.Value, targetPos, targetRot));
        }

        return objsInfo;
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

        List<NetInfo> dictionaryToSend = GetNetInfoDictionaryList();
        if (dictionaryToSend.Count <= 0) { return; }

        PacketHandler.SendPacket(serverSocket, ipep, PacketType.netObjsDictionary, dictionaryToSend);
        BroadCastPacket(PacketType.netObjsDictionary, dictionaryToSend, ipep);
    }

    void HandlePlayerActions(Wrappers.PlayerActionList actionsListContainer, EndPoint senderEP)
    {
        BroadCastPacket(PacketType.playerActionsList, actionsListContainer, senderEP);
    }
}
