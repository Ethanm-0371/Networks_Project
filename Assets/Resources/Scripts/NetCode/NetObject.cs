using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetObject : MonoBehaviour
{
    public int netID { get; private set; }
    public string prefabPath { get; private set; }

    private void Awake()
    {
        //prefabPath = getasset
    }
}
