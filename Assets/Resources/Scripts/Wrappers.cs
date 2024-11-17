using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrappers
{
    [Serializable]
    public struct PlayerActionList
    {
        public PlayerActionList(uint playerID, List<PlayerActions> actionList)
        {
            id = playerID;
            l = new List<PlayerActions>(actionList);
        }

        public uint id; //Id of the player
        public List<PlayerActions> l; //List of actions
    }

    [Serializable]
    public struct PlayerActions
    {
        public enum Actions
        {
            None,
            MoveF,
            MoveB,
            MoveL,
            MoveR,
            Rotate,
        }

        public PlayerActions(List<Actions> actionsInOneFrame, string parameter)
        {
            a = actionsInOneFrame;
            p = parameter;
        }

        public List<Actions> a; //Action type
        public string p; //Parameter
    }

    [Serializable]
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

    [Serializable]
    public struct ListWrapper<T>
    {
        public List<T> _list;

        public ListWrapper(List<T> list)
        {
            _list = list;
        }
    }
}