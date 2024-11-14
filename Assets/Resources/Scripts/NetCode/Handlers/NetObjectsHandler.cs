using System;
using System.Collections.Generic;
using UnityEngine;
using Wrappers;

public class NetObjectsHandler : MonoBehaviour
{
    Dictionary<Type, string> prefabPaths = new Dictionary<Type, string>()
    {
        { typeof(object),        "" },
        { typeof(PlayerWrapper), "PlayerPrefab" },
    };

    public Dictionary<uint, GameObject> netGameObjects = new Dictionary<uint, GameObject>();

    List<(uint, object)> netObjectsToInstantiate = new List<(uint, object)>();  //Change to queue structure for performance?
    List<(uint, object)> netObjectsToUpdate = new List<(uint, object)>();       //Change to queue structure for performance?

    private void Update()
    {
        //TO DO: Lock queue
        if (netObjectsToInstantiate.Count > 0)
        {
            foreach (var obj in netObjectsToInstantiate)
            {
                InternalInstantiate(obj.Item1, obj.Item2);
            }
            netObjectsToInstantiate.Clear();
        }

        if (netObjectsToUpdate.Count > 0)
        {
            foreach (var obj in netObjectsToUpdate)
            {
                netGameObjects[obj.Item1].GetComponent<NetObject>().UpdateObject(obj.Item2);
            }
            netObjectsToUpdate.Clear();
        }
    }

    public void CheckNetObjects(Dictionary<uint, object> receivedDictionary)
    {
        foreach (var entry in receivedDictionary)
        {
            if (netGameObjects.ContainsKey(entry.Key))
            {
                netObjectsToUpdate.Add((entry.Key, entry.Value));
            }
            else
            {
                netObjectsToInstantiate.Add((entry.Key, entry.Value));
            }
        }
    }

    private void InternalInstantiate(uint netID, object objectToInstantiate)
    {
        Type objectType = objectToInstantiate.GetType();
        GameObject newNetObj = (GameObject)Instantiate(Resources.Load("Prefabs/" + prefabPaths[objectType]));

        newNetObj.GetComponent<NetObject>().netID = netID;
        netGameObjects.Add(netID, newNetObj);

        if (objectType == typeof(PlayerWrapper))
        {
            InitPlayer(newNetObj, (PlayerWrapper)objectToInstantiate);
        }

    }

    private void InitPlayer(GameObject GO, PlayerWrapper info)
    {
        if (info.o != GameClient.Singleton.userName) { return; }

        GO.GetComponent<PlayerBehaviour>().isOwner = true;
        //Also set camera and others.
    }
}
