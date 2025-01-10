using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Manager : MonoBehaviour
{
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
    }

    private void OnEnable()
    {
        gameObject.AddComponent<EntityManager>();
    }

    private void OnDisable()
    {
        Destroy(GetComponent<EntityManager>());
    }
}
