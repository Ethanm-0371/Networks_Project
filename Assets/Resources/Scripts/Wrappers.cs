using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrappers
{
    public struct PlayerAction
    {
        public enum Actions
        {
            None,
            Move,
            Rotate,
        }

        public PlayerAction(Actions action, params object[] parameters)
        {
            a = action;
            p = parameters;
        }

        public Actions a; //Action type
        public object[] p; //Parameters
    }

    public struct Player
    {
        public Player(uint netID, string ownerName)
        {
            id = netID;
            o = ownerName;
            p = Vector3.zero;
            r = Quaternion.identity;
        }
        public Player(PlayerBehaviour instance)
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

    //[Serializable] ?
    public struct ListWrapper<T>
    {
        public List<T> _list;

        public ListWrapper(List<T> list)
        {
            _list = list;
        }
    }
}