using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct PlayerWrapper
{
    public PlayerWrapper(PlayerBehaviour instance)
    {
        id = instance.netID;
        p = instance.transform.position;
        r = instance.transform.rotation;
    }

    // Shortened names equals more space to add in buffer
    public uint id;
    public Vector3 p;
    public Quaternion r;
}

public class PlayerBehaviour : NetObject
{
    //hehe
}
