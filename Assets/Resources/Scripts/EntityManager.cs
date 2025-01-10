using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    GameObject[] worldSpawns;
    GameObject[] roomSpawns;

    public List<Level2Manager.ZoneTrigger> roomGroups;

    private void Start()
    {
        worldSpawns = GameObject.FindGameObjectsWithTag("ZombieSpawnPoint");
        roomSpawns = GameObject.FindGameObjectsWithTag("ZombieSpawnRoom");

        StartCoroutine(SpawnZombie());

        for (int i = 0; i < worldSpawns.Length; i++)
        {
            GameServer.Singleton.AddNewNetObjectInfo(new Wrappers.BasicZombie(i, false));
        }
    }

    public Transform GetSpawner(bool isRoom, int index)
    {
        if (isRoom)
        {
            return roomSpawns[index].transform;
        }
        else
        {
            return worldSpawns[index].transform;
        }
    }

    IEnumerator SpawnZombie()
    {
        while(true)
        {
            yield return new WaitForSeconds(5.0f);

            foreach (var group in roomGroups)
            {
                if (!group.isActive) { continue; }

                foreach (var room in group.spawnRooms)
                {
                    //int roomNum = UnityEngine.Random.Range(0, roomSpawns.Length); ???
                    //(This is correct, just depends on wether implementation is random or not)

                    int roomPosInArray = Array.IndexOf(roomSpawns, room.gameObject);
                    GameServer.Singleton.AddNewNetObjectInfo(new Wrappers.BasicZombie(roomPosInArray, true));
                }
            }
        }
    }
}
