using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct PlayerWrapper
{
    PlayerWrapper(PlayerBehaviour instance)
    {
        NetID = instance.netID;
        position = instance.transform.position;
        rotation = instance.transform.rotation;
    }

    public int NetID;
    public Vector3 position;
    public Quaternion rotation;
}

public class PlayerBehaviour : NetObject
{
    //hehe
}
