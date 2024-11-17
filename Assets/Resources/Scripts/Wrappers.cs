using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrappers
{
    [Serializable]
    public struct PlayerActionList
    {
        public PlayerActionList(uint playerID, List<List<PlayerAction>> frameList)
        {
            var wrappedList = new ListWrapper<ListWrapper<PlayerAction>>(new List<ListWrapper<PlayerAction>>());

            foreach (var item in frameList)
            {
                wrappedList._list.Add(new ListWrapper<PlayerAction>(item));
            }

            id = playerID;
            l = wrappedList;
        }

        public uint id; //Id of the player which executed the actions
        public ListWrapper<ListWrapper<PlayerAction>> l;
    }

    [Serializable]
    public struct PlayerAction
    {
        public enum Actions
        {
            None,
            Move,
            Rotate,
        }

        public PlayerAction(Actions action, PlayerActionParams parameters)
        {
            a = action;
            p = parameters;
        }

        public Actions a; //Action type
        public PlayerActionParams p; //Parameters
    }

    [Serializable]
    public struct PlayerActionParams
    {
        public PlayerActionParams(params object[] parameters)
        {
            l = new List<string>();

            foreach (var item in parameters)
            {
                l.Add(JsonUtility.ToJson(item));
            }
        }

        public List<string> l; //Parameters list
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