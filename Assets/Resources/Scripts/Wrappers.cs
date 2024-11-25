using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Wrappers
{
    public struct Types
    {
        public enum ClassIdentifyers
        {
            None,
            UserData,
            PlayerActions,
            PlayerActionsList,
            Player,
            SceneLoadedData,
            NetObjInfo,
        }

        public static Dictionary<Type, ClassIdentifyers> encodeTypes = new Dictionary<Type, ClassIdentifyers>()
        {
            {typeof(object),                       ClassIdentifyers.None}, // Acts as the "null" equivalent
            {typeof(UserData),                     ClassIdentifyers.UserData},
            {typeof(PlayerActions),                ClassIdentifyers.PlayerActions},
            {typeof(PlayerActionList),             ClassIdentifyers.PlayerActionsList},
            {typeof(Player),                       ClassIdentifyers.Player},
            {typeof(SceneLoadedData),              ClassIdentifyers.SceneLoadedData},
            {typeof(NetObjInfo),                   ClassIdentifyers.NetObjInfo},
        };
        public static Dictionary<ClassIdentifyers, Type> decodeTypes = new Dictionary<ClassIdentifyers, Type>()
        {
            {ClassIdentifyers.None,                typeof(object)}, // Acts as the "null" equivalent
            {ClassIdentifyers.UserData,            typeof(UserData)},
            {ClassIdentifyers.PlayerActions,       typeof(PlayerActions)},
            {ClassIdentifyers.PlayerActionsList,   typeof(PlayerActionList)},
            {ClassIdentifyers.Player,              typeof(Player)},
            {ClassIdentifyers.SceneLoadedData,     typeof(SceneLoadedData)},
            {ClassIdentifyers.NetObjInfo,          typeof(NetObjInfo)},
        };
    }

    [Serializable]
    public struct UserData : NetInfo
    {
        public string userName;

        public UserData(string username)
        {
            userName = username;
        }

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(userName);

            byte[] data = stream.ToArray();

            stream.Close();
            writer.Close();

            return data;
        }
        public void Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            userName = reader.ReadString();

            stream.Close();
        }
    }

    [Serializable]
    public struct PlayerActions : NetInfo
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

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((short)a.Count);

            foreach (var item in a)
            {
                writer.Write((char)item);
            }

            writer.Write(p);

            byte[] data = stream.ToArray();

            stream.Close();
            writer.Close();

            return data;
        }
        public void Deserialize(byte[] data)
        {
            a = new List<Actions>();

            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            short actionAmount = reader.ReadInt16();

            for (int i = 0; i < actionAmount; i++)
            {
                a.Add((Actions)reader.ReadChar());
            }

            p = reader.ReadString();

            stream.Close();
        }
    }

    [Serializable]
    public struct PlayerActionList : NetInfo
    {
        public PlayerActionList(uint playerID, List<PlayerActions> actionList)
        {
            id = playerID;
            l = new List<PlayerActions>(actionList);
        }

        public uint id; //Id of the player
        public List<PlayerActions> l; //List of actions

        public byte[] Serialize()
        {
            byte[] serializedID = BitConverter.GetBytes(id);
            byte[] numberOfActions = BitConverter.GetBytes((short)l.Count);

            List<byte[]> serializedActions = new List<byte[]>();
            int totalSize = 6; //4 for the id uint and 2 for the num of entries that will always be there

            foreach (var item in l)
            {
                byte[] actionToAdd = item.Serialize();
                byte[] size = BitConverter.GetBytes((short)actionToAdd.Length);

                serializedActions.Add(size);
                serializedActions.Add(actionToAdd);

                totalSize += actionToAdd.Length + 2;
            }

            byte[] result = new byte[totalSize];
            int offset = 6; //Id and number of entries

            Buffer.BlockCopy(serializedID, 0, result, 0, 4);
            Buffer.BlockCopy(numberOfActions, 0, result, 4, 2);

            foreach (var item in serializedActions)
            {
                Buffer.BlockCopy(item, 0, result, offset, item.Length);
                offset += item.Length;
            }

            return result;
        }

        public void Deserialize(byte[] data)
        {
            id = BitConverter.ToUInt32(data, 0);
            short numberOfActions = BitConverter.ToInt16(data, 4);

            int offset = 6;

            for (int i = 0; i < numberOfActions; i++)
            {
                l = new List<PlayerActions>();

                short actionSize = BitConverter.ToInt16(data, offset);
                offset += 2;

                byte[] actionToDecode = new byte[actionSize];
                Buffer.BlockCopy(data, offset, actionToDecode, 0, actionSize);

                PlayerActions actionsToAdd = new PlayerActions();

                actionsToAdd.Deserialize(actionToDecode);

                l.Add(actionsToAdd);
            }
        }
    }

    [Serializable]
    public struct Player : NetInfo
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

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(id);
            writer.Write(o);

            writer.Write(p.x);
            writer.Write(p.y);
            writer.Write(p.z);

            writer.Write(r.x);
            writer.Write(r.y);
            writer.Write(r.z);
            writer.Write(r.w);

            byte[] data = stream.ToArray();

            stream.Close();
            writer.Close();

            return data;
        }

        public void Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            id = reader.ReadUInt32();
            o = reader.ReadString();

            p.x = reader.ReadSingle();
            p.y = reader.ReadSingle();
            p.z = reader.ReadSingle();
            
            r.x = reader.ReadSingle();
            r.y = reader.ReadSingle();
            r.z = reader.ReadSingle();
            r.w = reader.ReadSingle();

            stream.Close();
        }
    }

    [Serializable]
    public struct SceneLoadedData : NetInfo
    {
        //This variable is completely unnecessary
        int placeHolder;

        public SceneLoadedData(int value)
        {
            placeHolder = value;
        }

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(placeHolder);

            byte[] data = stream.ToArray();

            stream.Close();
            writer.Close();

            return data;
        }
        public void Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            placeHolder = reader.ReadInt32();

            stream.Close();
        }
    }

    [Serializable]
    public struct NetObjInfo : NetInfo
    {
        public uint id;
        public NetInfo objectInfo;

        public NetObjInfo(uint id, NetInfo objectInfo)
        {
            this.id = id;
            this.objectInfo = objectInfo;
        }

        public byte[] Serialize()
        {
            byte[] serializedID = BitConverter.GetBytes(id);

            byte[] serializedType = new byte[1]; 
            serializedType[0] = (byte)Types.encodeTypes[objectInfo.GetType()];

            byte[] serializedClass = objectInfo.Serialize();

            byte[] result = new byte[serializedID.Length + 1 + serializedClass.Length];

            Buffer.BlockCopy(serializedID, 0, result, 0, serializedID.Length);
            Buffer.BlockCopy(serializedType, 0, result, serializedID.Length, 1);
            Buffer.BlockCopy(serializedClass, 0, result, serializedID.Length + 1, serializedClass.Length);

            return result;
        }

        public void Deserialize(byte[] data)
        {
            id = BitConverter.ToUInt32(data, 0);

            Types.ClassIdentifyers classType = (Types.ClassIdentifyers)data[4];

            byte[] classToDecode = new byte[data.Length - 5]; //4 from the uint and 1 from the type
            Buffer.BlockCopy(data, 5, classToDecode, 0, data.Length - 5);

            NetInfo classInstance = (NetInfo)Activator.CreateInstance(Types.decodeTypes[classType]);
            classInstance.Deserialize(classToDecode);

            objectInfo = classInstance;
        }
    }
}