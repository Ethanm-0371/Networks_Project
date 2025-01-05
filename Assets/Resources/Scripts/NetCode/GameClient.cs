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
    Thread mainReceivingThread;

    public string userName;
    public GameObject ownedPlayerGO = null;
    float playerActionsSendFrequency = 0.2f;

    // Ping variables
    Coroutine pingCoroutine;
    bool pingReceived = false;
    float timeoutDuration = 30.0f; // For debugging, change to 1.0f, default 30.0f

    Queue<(PacketType, object)> functionsQueue = new Queue<(PacketType, object)>();
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

        if (functionsQueue.Count > 0)
        {
            //Maybe add foreach or while != 0
            (PacketType, object) dequeuedFunction = functionsQueue.Dequeue();
            functionsDictionary[dequeuedFunction.Item1](dequeuedFunction.Item2);
        }
    }

    private void OnDestroy()
    {
        mainReceivingThread.Abort(); //Forces thread termination before cleaning sockets

        if (pingCoroutine != null) 
            StopCoroutine(pingCoroutine);

        clientSocket.Shutdown(SocketShutdown.Both); //Disables sending and receiving
        clientSocket.Close(); //Closes the connection and frees all associated resources
        pingReceived = false;
    }

    #region Client Functions

    public void Init(string ip, string username)
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), 9050); //Change port to another inputfield

        // Send ping
        pingCoroutine = StartCoroutine(PingServer(timeoutDuration));

        //Handshake
        PacketHandler.SendPacket(clientSocket, serverEndPoint, PacketType.PlayerData, new Wrappers.UserData(username));
        userName = username;

        mainReceivingThread = new Thread(Receive);
        mainReceivingThread.Start();

        StartCoroutine(SendPlayerActions(playerActionsSendFrequency));
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

            (PacketType, object) decodedClass;

            //Data[1] defines if the packet contains a list
            if (data[1] != 0) { decodedClass = PacketHandler.DecodeMultiPacket(data); }
            else { decodedClass = PacketHandler.DecodeSinglePacket(data); }

            functionsQueue.Enqueue(decodedClass);
        }
    }

    IEnumerator PingServer(float timeoutDuration)
    {
        while (true)
        {
            Debug.Log("Sent ping");
            PacketHandler.SendPacket(clientSocket, serverEndPoint, PacketType.Ping, new Wrappers.PingData(0));

            yield return new WaitForSeconds(timeoutDuration);

            if (!pingReceived)
            {
                Debug.Log("Ping not received from server. Destroying now...");
                OnDestroy();
                break;
            }
        }
    }

    IEnumerator SendPlayerActions(float sendFrequency)
    {
        while (true)
        {
            if (ownedPlayerGO == null) { yield return null; }
            else
            {
                var actionList = ownedPlayerGO.GetComponent<PlayerBehaviour>().GetActionsList();
                //var playerBehaviour = ownedPlayerGO.GetComponent<PlayerBehaviour>();
                //var changes = playerBehaviour.GetChangedComponents();

                if (actionList != null)
                {
                    PacketHandler.SendPacket(clientSocket, serverEndPoint, PacketType.playerActionsList, actionList);
                    //PacketHandler.SendPacket(clientSocket, serverEndPoint, PacketType.playerActionsList, changes);
                }

                //yield return new WaitForSeconds(sendFrequency);
                yield return null;
            }
        }
    }

    #endregion

    #region HandlerFunctions

    void StartClientFunctions()
    {
        functionsDictionary = new Dictionary<PacketType, Action<object>>()
        {
            { PacketType.Ping, obj => { HandlePing(); } },
            { PacketType.ChangeSceneCommand, obj => { HandleSceneChange((Wrappers.ChangeSceneCommand)obj); } },
            { PacketType.netObjsDictionary, obj => { HandleReceiveNetObjects((List<NetInfo>)obj); } },
            { PacketType.playerActionsList, obj => { HandlePlayerActions((Wrappers.PlayerActionList)obj); } },
        };
    }

    void HandlePing() 
    {
        Debug.Log("Ping received from server.");
        pingReceived = true;
    }

    void HandleSceneChange(Wrappers.ChangeSceneCommand command)
    {
        netObjsHandler.netGameObjects.Clear();
        ScenesHandler.Singleton.LoadScene(command.targetSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    void HandleReceiveNetObjects(List<NetInfo> netObjectsInfo)
    {
        netObjsHandler.CheckNetObjects(netObjectsInfo);

        ScenesHandler.Singleton.SetReady();
    }
    void HandlePlayerActions(Wrappers.PlayerActionList list)
    {
        foreach (var actions in list.l)
        {
            netObjsHandler.netGameObjects[list.id].GetComponent<PlayerBehaviour>().ExecuteActions(actions);
        }
    }
    
    #endregion
}
