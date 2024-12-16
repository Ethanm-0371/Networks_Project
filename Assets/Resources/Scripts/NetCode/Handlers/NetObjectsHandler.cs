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
        List<Wrappers.ObjectToDestroy> objectsToDestroy = new List<Wrappers.ObjectToDestroy>();
        foreach (var item in receivedList)
        {
            if (item is Wrappers.ObjectToDestroy)
            {
                var obj = (Wrappers.ObjectToDestroy)item;

                objectsToDestroy.Add(obj);
            }
        }

        //Delete all marked objects
        foreach (var item in objectsToDestroy)
        {
            Destroy(netGameObjects[item.netID]);
            netGameObjects.Remove(item.netID);

            receivedList.Remove(item);
        }

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

        if (objectType == typeof(Wrappers.Player))
        {
            newNetObj.GetComponent<PlayerBehaviour>().InitPlayer((Wrappers.Player)objectToInstantiate);
        }
        if (objectType == typeof(Wrappers.BasicZombie))
        {
            var castWrapper = (Wrappers.BasicZombie)objectToInstantiate;

            newNetObj.GetComponent<BasicEnemy>().InitZombie(castWrapper.isRoomZombie, castWrapper.spawnPoint);
        }
    }
}
