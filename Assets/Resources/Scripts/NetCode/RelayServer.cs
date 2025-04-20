using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CustomExtensions;

public class RelayServer : MonoBehaviour
{

    Socket relaySocket;
    IPEndPoint host;
    const int port = 9050;
    List<IPEndPoint> connectedIps = new List<IPEndPoint>();

    Thread receivingThread;

    Dictionary<PacketType, Action<object, EndPoint>> functionsDictionary;

    Queue<(PacketType, object, EndPoint)> functionsQueue = new Queue<(PacketType, object, EndPoint)>();

    void Awake()
    {
        StartRelayServerFunctions();
    }

    void Start()
    {
        relaySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);

        relaySocket.Bind(ipep);

        // Starts receiving messages from host/clients
        receivingThread = new Thread(Receive);
        receivingThread.Start();

        Debug.Log($"Relay server started on port: {port}.");
    }

    void Update()
    {
        // Functions queue
        while (functionsQueue.Count > 0)
        {
            // Employs the functions from the functions dictionary
            // In Relay Server, only Disconnect should be handled
            (PacketType, object, EndPoint) dequeuedFunction = functionsQueue.Dequeue();
            functionsDictionary[dequeuedFunction.Item1](dequeuedFunction.Item2, dequeuedFunction.Item3);
        }
    }

    // Relay Server functions --------------------------------------------------------------------------
    #region Relay Server functions
    void StartRelayServerFunctions()
    {
        functionsDictionary = new Dictionary<PacketType, Action<object, EndPoint>>()
        {
            { PacketType.Disconnect,        (obj, ep) => { HandleDisconnect(ep); } },
        };
    }

    void Receive()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);
        byte[] data = new byte[1024];
        int recv;

        while (true)
        {
            try
            {
                recv = relaySocket.ReceiveFrom(data, ref Remote);
                if (recv == 0) { continue; }
            }
            catch (SocketException ex)
            {
                Debug.LogWarning("SocketException error (RelayServer Receive()): " + ex.Message);
            }

            Debug.Log($"New client connected: {Remote}");

            // Add IP if it does not exist
            if (!connectedIps.Contains(sender)) connectedIps.Add(sender);
            
            (PacketType, object) decodedClass;

            // Data[1] defines if the packet contains a list
            // 0 - Single packet
            // 1 - Multi packet
            if (data[1] != 0) decodedClass = PacketHandler.DecodeMultiPacket(data);
            else decodedClass = PacketHandler.DecodeSinglePacket(data);


            if (decodedClass.Item1.Equals(PacketType.Disconnect))
            {
                functionsQueue.Enqueue((decodedClass.Item1, decodedClass.Item2, Remote));
            }

            // Re send packet to all clients
            foreach (IPEndPoint client in connectedIps)
            {
                relaySocket.SendTo(data, data.Length, SocketFlags.None, client);
            }
        }
    }

    void HandleDisconnect(EndPoint ep)
    {
        IPEndPoint ipep = ep as IPEndPoint;
        if (connectedIps.Contains(ipep))
        {
            connectedIps.Remove(ipep);
            Debug.Log($"IP: {ep} removed.");
        }
        else
        {
            Debug.Log($"IP: {ep} can't be found.");
        }

        Debug.Log("--------------------------------------------------------------------------");
        Debug.Log($"Remaining users: {connectedIps.Count}");
        foreach (var user in connectedIps)
        {
            Debug.Log($"IP: {user.GetIP()}, Port: {user.GetPort()}");
        }
    }
    #endregion
}
