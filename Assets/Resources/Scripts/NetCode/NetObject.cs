using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetObject : MonoBehaviour
{
    public uint netID { get; set; }

    public abstract NetInfo GetNetInfo();
    public abstract void UpdateObject(NetInfo info);
}
