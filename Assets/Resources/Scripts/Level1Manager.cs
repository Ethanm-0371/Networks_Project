using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Manager : MonoBehaviour
{
    [SerializeField] ExtractionZone extractionZone;
    int playersOnExtraction = 0;

    private void Start()
    {
        extractionZone.PlayerEnteredZone.AddListener(() => 
        { 
            playersOnExtraction++;
            if (playersOnExtraction >= GameServer.Singleton.GetNumberOfPlayers())
            {
                GameServer.Singleton.EndGame();
            }
        });
        extractionZone.PlayerLeftZone.AddListener(() => { playersOnExtraction--; });
    }
}
