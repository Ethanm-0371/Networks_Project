using System;
using System.Collections.Generic;
using UnityEngine;

public class NetObjectsHandler : MonoBehaviour
{
    Dictionary<Type, string> prefabPaths = new Dictionary<Type, string>()
    {
        { typeof(object),        "" },
        { typeof(Wrappers.Player), "PlayerPrefab" },
    };

    public Dictionary<uint, GameObject> netGameObjects = new Dictionary<uint, GameObject>();

    public void CheckNetObjects(Dictionary<uint, object> receivedDictionary)
    {
        foreach (var entry in receivedDictionary)
        {
            if (netGameObjects.ContainsKey(entry.Key))
            {
                netGameObjects[entry.Key].GetComponent<NetObject>().UpdateObject(entry.Value);
            }
            else
            {
                InstantiateGameObject(entry.Key, entry.Value);
            }
        }
    }

    private void InstantiateGameObject(uint netID, object objectToInstantiate)
    {
        Type objectType = objectToInstantiate.GetType();
        GameObject newNetObj = (GameObject)Instantiate(Resources.Load("Prefabs/" + prefabPaths[objectType]));

        newNetObj.GetComponent<NetObject>().netID = netID;
        netGameObjects.Add(netID, newNetObj);

        if (objectType == typeof(Wrappers.Player))
        {
            InitPlayer(newNetObj, (Wrappers.Player)objectToInstantiate);
        }
    }

    private void InitPlayer(GameObject GO, Wrappers.Player info)
    {
        if (info.o != GameClient.Singleton.userName) { return; }

        GO.GetComponent<PlayerBehaviour>().isOwner = true;
        GameClient.Singleton.ownedPlayerGO = GO;
        //Also set camera and others.
    }
}
