using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExtractionZone : MonoBehaviour
{
    public UnityEvent PlayerEnteredZone;
    public UnityEvent PlayerLeftZone;

    private void OnTriggerEnter(Collider other)
    {
        PlayerEnteredZone.Invoke();
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerLeftZone.Invoke();
    }
}
