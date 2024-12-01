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

    public void CheckNetObjects(List<NetInfo> receivedList)
    {
        foreach (Wrappers.NetObjInfo entry in receivedList)
        {
            if (netGameObjects.ContainsKey(entry.id))
            {
                netGameObjects[entry.id].GetComponent<NetObject>().UpdateObject(entry.objectInfo);
            }
            else
            {
                InstantiateGameObject(entry.id, entry.objectInfo);
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
        if (info.o != GameClient.Singleton.userName) 
        {
            GO.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = info.o;
            return; 
        }

        GO.GetComponent<PlayerBehaviour>().isOwner = true;
        GameClient.Singleton.ownedPlayerGO = GO;

        GO.GetComponent<PlayerBehaviour>().AttachCamera();

        GO.GetComponentInChildren<Canvas>().gameObject.SetActive(false);
    }
}
