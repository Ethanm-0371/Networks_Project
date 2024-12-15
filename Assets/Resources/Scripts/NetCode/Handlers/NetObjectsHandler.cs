using System;
using System.Collections.Generic;
using UnityEngine;

public class NetObjectsHandler : MonoBehaviour
{
    Dictionary<Type, string> prefabPaths = new Dictionary<Type, string>()
    {
        { typeof(object),        "" },
        { typeof(Wrappers.Player), "PlayerPrefab" },
        { typeof(Wrappers.BasicZombie), "BasicEnemyPrefab" },
    };

    public Dictionary<uint, GameObject> netGameObjects = new Dictionary<uint, GameObject>();

    public void CheckNetObjects(List<NetInfo> receivedList)
    {
        //Must add a way to read objects that need to be destroyed.

        foreach (Wrappers.NetObjInfo entry in receivedList)
        {
            if (netGameObjects.ContainsKey(entry.id))
            {
                netGameObjects[entry.id].transform.position = entry.position;
                netGameObjects[entry.id].transform.rotation = entry.rotation;
                netGameObjects[entry.id].GetComponent<NetObject>().UpdateObject(entry.objectInfo);
            }
            else
            {
                InstantiateGameObject(entry.id, entry.objectInfo, entry.position, entry.rotation);
            }
        }
    }

    private void InstantiateGameObject(uint netID, object objectToInstantiate, Vector3 position, Quaternion rotation)
    {
        Type objectType = objectToInstantiate.GetType();
        GameObject newNetObj = (GameObject)Instantiate(Resources.Load("Prefabs/" + prefabPaths[objectType]), position, rotation);

        newNetObj.GetComponent<NetObject>().netID = netID;

        netGameObjects.Add(netID, newNetObj);
        newNetObj.GetComponent<NetObject>().OnDestroyObject.AddListener(() => 
        { 
            netGameObjects.Remove(netID);
            GameServer.Singleton?.RemoveNetObjectInfo(netID);
        });

        if (objectType == typeof(Wrappers.Player))
        {
            newNetObj.GetComponent<PlayerBehaviour>().InitPlayer((Wrappers.Player)objectToInstantiate);
        }
        if (objectType == typeof(Wrappers.BasicZombie))
        {
            var castWrapper = (Wrappers.BasicZombie)objectToInstantiate;

            GameObject[] spawnList = GameObject.FindGameObjectsWithTag("SpawnPoint");

            newNetObj.transform.position = spawnList[castWrapper.spawnPoint].transform.position;
        }
    }
}
