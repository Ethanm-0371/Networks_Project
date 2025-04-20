using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class RelayServer : MonoBehaviour
{
    UdpClient udpServer;
    Dictionary<IPEndPoint, IPEndPoint> relayTable = new Dictionary<IPEndPoint, IPEndPoint>();
    IPEndPoint host;
    const int port = 9050;

    void Start()
    {
        udpServer = new UdpClient(port);
        Debug.Log($"Relay server started on port: {port}.");

        while (true)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpServer.Receive(ref remoteEP);

            // Identify who sent it and who it is meant for (if protocol includes that)
            if (!relayTable.ContainsKey(remoteEP))
            {
                Debug.Log($"New client connected: {remoteEP}");
                // This needs fixing. But at least, it reads in relay server.
                foreach (var ep in relayTable.Keys)
                {
                    if (!ep.Equals(remoteEP))
                    {
                        relayTable[remoteEP] = ep;
                        relayTable[ep] = remoteEP;
                        break;
                    }
                }
            }

            if (relayTable.ContainsKey(remoteEP))
            {
                var targetEP = relayTable[remoteEP];
                udpServer.Send(data, data.Length, targetEP);
            }

        }
    }
}
