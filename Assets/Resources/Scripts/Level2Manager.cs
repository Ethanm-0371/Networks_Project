using System;
using System.Collections.Generic;
using UnityEngine;

public class Level2Manager : MonoBehaviour
{
    [Serializable]
    public class ZoneTrigger
    {
        public TriggerEventCaller trigger;
        public bool isActive = false;
        public List<Transform> spawnRooms;
    }

    [SerializeField] List<ZoneTrigger> zoneColliders = new List<ZoneTrigger>();

    EntityManager entityManager;

    [SerializeField] TriggerEventCaller extractionZone;
    int playersOnExtraction = 0;

    private void Start()
    {
        extractionZone.PlayerEnteredTrigger.AddListener(() => 
        { 
            playersOnExtraction++;
            if (playersOnExtraction >= GameServer.Singleton.GetNumberOfPlayers())
            {
                GameServer.Singleton.EndGame();
            }
        });
        extractionZone.PlayerLeftTrigger.AddListener(() => { playersOnExtraction--; });


        InitRoomZones();
    }

    private void OnEnable()
    {
        entityManager = gameObject.AddComponent<EntityManager>();
        entityManager.roomGroups = zoneColliders;
    }

    private void OnDisable()
    {
        Destroy(entityManager);
        entityManager = null;
    }

    private void InitRoomZones()
    {
        foreach (var zone in zoneColliders)
        {
            zone.trigger.PlayerEnteredTrigger.AddListener(() =>
            {
                zone.isActive = true;
            });
            zone.trigger.PlayerLeftTrigger.AddListener(() =>
            {
                if (zone.trigger.playersInside <= 0) { zone.isActive = false; }
            });
        }
    }
}
