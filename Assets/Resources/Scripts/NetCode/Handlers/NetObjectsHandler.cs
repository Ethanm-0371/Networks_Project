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

    public Dictionary<int, object> netObjects = new Dictionary<int, object>();

    List<(int, object)> netObjectsQueue = new List<(int, object)>();

    private void Update()
    {
        if (netObjectsQueue.Count > 0)
        {
            foreach (var obj in netObjectsQueue)
            {
                InternalInstantiate(obj.Item1, obj.Item2);
            }
            netObjectsQueue.Clear();
        }
    }

    public void CheckNetObjects(Dictionary<int, object> receivedDictionary)
    {
        foreach (var entry in receivedDictionary)
        {
            if (netObjects.ContainsKey(entry.Key))
            {
                //Update object
            }
            else
            {
                InstantiateNetObject(entry.Value, entry.Key);
            }
        }
    }

    public void InstantiateNetObject(object objectToInstantiate, int netID = 0)
    {
        netObjectsQueue.Add((netID, objectToInstantiate));
    }
    private void InternalInstantiate(int netID, object objectToInstantiate)
    {
        GameObject newNetObj = (GameObject)Instantiate(Resources.Load("Prefabs/" + prefabPaths[objectToInstantiate.GetType()]));

        if (netID == 0) 
        { 
            newNetObj.GetComponent<NetObject>().netID = newNetObj.GetInstanceID();
            netObjects.Add(netID, objectToInstantiate);
            return; 
        }

        //Temporary
        PlayerWrapper objInfo = (PlayerWrapper)objectToInstantiate;
        newNetObj.GetComponent<NetObject>().netID = objInfo.NetID;
    }
}
