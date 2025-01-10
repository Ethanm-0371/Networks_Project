using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEventCaller : MonoBehaviour
{
    [HideInInspector] public UnityEvent PlayerEnteredTrigger;
    [HideInInspector] public UnityEvent PlayerLeftTrigger;

    public int playersInside { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") { return; }

        playersInside++;
        PlayerEnteredTrigger.Invoke();
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player") { return; }

        playersInside--;
        PlayerLeftTrigger.Invoke();
    }
}
