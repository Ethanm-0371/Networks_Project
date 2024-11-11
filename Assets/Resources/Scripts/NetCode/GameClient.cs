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
    Dictionary<Type, Action<object>> functionsDictionary;

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

        functionsDictionary = new Dictionary<Type, Action<object>>()
        {
            { typeof(Dictionary<uint, object>), obj => { HandleReceiveNetObjects((Dictionary<uint, object>)obj); } }, //Change later
            { typeof(ConnectionTest1), obj => { Debug.Log("Client received a Test1"); } }, //Change later
            { typeof(ConnectionTest2), obj => { Debug.Log("Client received a Test2"); } }
        };

        netObjsHandler = gameObject.AddComponent<NetObjectsHandler>();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            PacketHandler.SendPacket(clientSocket, serverEndPoint, new ConnectionTest1(7, "Mondongo")); //Debug only
            Debug.Log("Client sent a Test1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PacketHandler.SendPacket(clientSocket, serverEndPoint, new ConnectionTest2("Goku calvo", Vector3.one)); //Debug only
            Debug.Log("Client sent a Test2");
        }
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
        PacketHandler.SendPacket(clientSocket, serverEndPoint, new PlayerData(username));

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

            var decodedClass = PacketHandler.DecodeData(data);

            functionsDictionary[decodedClass.GetType()](decodedClass);
        }
    }

    void HandleReceiveNetObjects(Dictionary<uint, object> netObjects)
    {
        netObjsHandler.CheckNetObjects(netObjects);

        ScenesHandler.Singleton.SetReady();
    }
}
