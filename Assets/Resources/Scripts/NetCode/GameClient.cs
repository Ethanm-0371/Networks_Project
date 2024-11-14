using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class GameClient : MonoBehaviour
{
    public static GameClient Singleton { get; private set; }

    public Socket clientSocket;
    public IPEndPoint serverEndPoint;

    public string userName;

    Dictionary<PacketType, Action<object>> functionsDictionary;

    NetObjectsHandler netObjsHandler;

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

        StartClientFunctions();

        netObjsHandler = gameObject.AddComponent<NetObjectsHandler>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ScenesHandler.Singleton.LoadScene("Main_Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public void Init(string ip, string username)
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), 9050); //Change port to another inputfield

        //Handshake
        PacketHandler.SendPacket(clientSocket, serverEndPoint, PacketType.PlayerData, new PlayerData(username));
        userName = username;

        Thread receive = new Thread(Receive);
        receive.Start();
    }
    void Receive()
    {
        IPEndPoint sender = new IPEndPoint(serverEndPoint.Address, 0);
        EndPoint Remote = (EndPoint)(sender);

        byte[] data = new byte[1024];
        int recv;

        while (true)
        {
            recv = clientSocket.ReceiveFrom(data, ref Remote);

            if (recv == 0) { continue; }

            (PacketType, object) decodedClass = PacketHandler.DecodeData(data);

            functionsDictionary[decodedClass.Item1](decodedClass.Item2);
        }
    }

    void StartClientFunctions()
    {
        functionsDictionary = new Dictionary<PacketType, Action<object>>()
        {
            { PacketType.netObjsDictionary, obj => { HandleReceiveNetObjects((Dictionary<uint, object>)obj); } },
        };
    }

    void HandleReceiveNetObjects(Dictionary<uint, object> netObjects)
    {
        netObjsHandler.CheckNetObjects(netObjects);

        ScenesHandler.Singleton.SetReady();
    }
}
