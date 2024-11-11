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
    Dictionary<Type, Action<object, EndPoint>> functionsDictionary;

    NetObjectsHandler netObjsHandler;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Singleton = this;
        }

        DontDestroyOnLoad(this.gameObject);

        functionsDictionary = new Dictionary<Type, Action<object, EndPoint>>()
        {
            { typeof(PlayerData), (obj, ep) => { AddUserToDictionary(ep, (PlayerData)obj); } },
            { typeof(SceneLoadedData), (obj, ep) => { HandleClientSceneLoaded(ep); } },
            { typeof(ConnectionTest1), (obj, ep) => { Debug.Log("Server received a Test1 from " + connectedUsers[ep]); BroadCastPacket((ConnectionTest1)obj, ep); } }, //Change later
            { typeof(ConnectionTest2), (obj, ep) => { Debug.Log("Server received a Test2 from " + connectedUsers[ep]); BroadCastPacket((ConnectionTest2)obj, ep);} }
        };
    }

    public void Init()
    {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

        serverSocket.Bind(ipep);

        //Start a new thread to receive messages.
        Thread mainThread = new Thread(Receive);
        mainThread.Start();

        netObjsHandler = gameObject.GetComponent<NetObjectsHandler>();
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

            var decodedClass = PacketHandler.DecodeData(data);

            functionsDictionary[decodedClass.GetType()](decodedClass, Remote);
        }
    }

    void BroadCastPacket<T>(T packetToSend, EndPoint sender)
    {
        foreach (var user in connectedUsers)
        {
            if(user.Key.GetIP().ToString() == sender.GetIP().ToString() &&
               user.Key.GetPort() == sender.GetPort()) { continue; }

            IPEndPoint ipep = new IPEndPoint(user.Key.GetIP(), user.Key.GetPort());
            PacketHandler.SendPacket(serverSocket, ipep, packetToSend);
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

    void AddUserToDictionary(EndPoint ep, PlayerData playerData)
    {
        connectedUsers.Add(ep, playerData.Username);
    }

    void HandleClientSceneLoaded(EndPoint ep)
    {
        netObjsHandler.AddNetObject(new PlayerWrapper(), netObjsHandler.GenerateRandomID());

        IPEndPoint ipep = new IPEndPoint(ep.GetIP(), ep.GetPort());

        byte[] encodedDictionary = PacketHandler.EncodeDictionary(netObjsHandler.netObjects);
        PacketHandler.SendPacket(serverSocket, ipep, encodedDictionary);
        BroadCastPacket(encodedDictionary, ep);
    }
}
