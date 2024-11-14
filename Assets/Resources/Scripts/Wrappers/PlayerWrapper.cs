using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrappers
{
    struct PlayerWrapper
    {
        public PlayerWrapper(uint netID, string ownerName)
        {
            id = netID;
            o = ownerName;
            p = Vector3.zero;
            r = Quaternion.identity;
        }
        public PlayerWrapper(PlayerBehaviour instance)
        {
            id = instance.netID;
            o = "";
            p = instance.transform.position;
            r = instance.transform.rotation;
        }

        // Shortened names equals more space to add in buffer
        public uint id;
        public string o; //Owner name
        public Vector3 p; //Position
        public Quaternion r; //Rotation
    }
}