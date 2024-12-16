using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    GameObject[] worldSpawns;
    GameObject[] roomSpawns;

    private void Start()
    {
        worldSpawns = GameObject.FindGameObjectsWithTag("ZombieSpawnPoint");
        roomSpawns = GameObject.FindGameObjectsWithTag("ZombieSpawnRoom");

        StartCoroutine(SpawnZombie());
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

            int room = UnityEngine.Random.Range(0, roomSpawns.Length);
            GameServer.Singleton.AddNewNetObjectInfo(new Wrappers.BasicZombie(room, true));
        }
    }
}
