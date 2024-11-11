using System;
using System.Collections.Generic;
using UnityEngine;

public class NetObjectsHandler : MonoBehaviour
{
    Dictionary<Type, string> prefabPaths = new Dictionary<Type, string>()
    {
        { typeof(object),        "" },
        { typeof(PlayerWrapper), "PlayerPrefab" },
    };

    public Dictionary<uint, object> netObjects = new Dictionary<uint, object>();

    List<(uint, object)> netObjectsQueue = new List<(uint, object)>();

    private void Update()
    {
        //TO DO: Lock queue
        if (netObjectsQueue.Count > 0)
        {
            foreach (var obj in netObjectsQueue)
            {
                InternalInstantiate(obj.Item1, obj.Item2);
            }
            netObjectsQueue.Clear();
        }
    }

    public void CheckNetObjects(Dictionary<uint, object> receivedDictionary)
    {
        foreach (var entry in receivedDictionary)
        {
            PlayerWrapper pw = (PlayerWrapper)entry.Value;

            if (netObjects.ContainsKey(entry.Key) && pw.id != 0)
            {
                //Update object
            }
            else
            {
                netObjectsQueue.Add((entry.Key, entry.Value));
            }
        }
    }

    public void AddNetObject(object obj, uint netID)
    {
        netObjects.Add(netID, obj);
    }

    private void InternalInstantiate(uint netID, object objectToInstantiate)
    {
        Type thing = objectToInstantiate.GetType();
        GameObject newNetObj = (GameObject)Instantiate(Resources.Load("Prefabs/" + prefabPaths[thing]));
        newNetObj.GetComponent<PlayerBehaviour>().netID = netID;

        //Temporary
        PlayerWrapper filledWrapper = new PlayerWrapper(newNetObj.GetComponent<PlayerBehaviour>());
        netObjects[netID] = filledWrapper;

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

        } while (netObjects.ContainsKey(id));

        // Convert the byte array to a uint
        return id;

    }
}
