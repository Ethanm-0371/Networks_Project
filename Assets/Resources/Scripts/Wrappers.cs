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
            BasicZombie,
            SceneLoadedData,
            NetObjInfo,
        }

        public static Dictionary<Type, ClassIdentifyers> encodeTypes = new Dictionary<Type, ClassIdentifyers>()
        {
            {typeof(object),                       ClassIdentifyers.None}, // Acts as the "null" equivalent
            {typeof(UserData),                     ClassIdentifyers.UserData},
            {typeof(ActionsInFrame),                ClassIdentifyers.PlayerActions},
            {typeof(PlayerActionList),             ClassIdentifyers.PlayerActionsList},
            {typeof(Player),                       ClassIdentifyers.Player},
            {typeof(BasicZombie),                  ClassIdentifyers.BasicZombie},
            {typeof(SceneLoadedData),              ClassIdentifyers.SceneLoadedData},
            {typeof(NetObjInfo),                   ClassIdentifyers.NetObjInfo},
        };
        public static Dictionary<ClassIdentifyers, Type> decodeTypes = new Dictionary<ClassIdentifyers, Type>()
        {
            {ClassIdentifyers.None,                typeof(object)}, // Acts as the "null" equivalent
            {ClassIdentifyers.UserData,            typeof(UserData)},
            {ClassIdentifyers.PlayerActions,       typeof(ActionsInFrame)},
            {ClassIdentifyers.PlayerActionsList,   typeof(PlayerActionList)},
            {ClassIdentifyers.Player,              typeof(Player)},
            {ClassIdentifyers.BasicZombie,         typeof(BasicZombie)},
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
    public struct PlayerAction : NetInfo
    {
        public enum ActionType
        {
            None,
            MoveF,
            MoveB,
            MoveL,
            MoveR,
            Rotate,
        }

        public ActionType actionType;
        public List<string> parameters;

        public PlayerAction(ActionType actionType)
        {
            this.actionType = actionType;
            parameters = new List<string>();
        }
        public PlayerAction(ActionType actionType, List<string> parameters)
        {
            this.actionType = actionType;
            this.parameters = parameters;
        }

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((char)actionType);

            writer.Write((short)parameters.Count);
            foreach (var param in parameters)
            {
                writer.Write(param);
            }

            byte[] data = stream.ToArray();

            stream.Close();
            writer.Close();

            return data;
        }

        public void Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            actionType = (ActionType)reader.ReadChar();
            parameters = new List<string>();

            short paramAmount = reader.ReadInt16();
            for (int i = 0; i < paramAmount; i++)
            {
                parameters.Add(reader.ReadString());
            }

            stream.Close();
        }
    }

    [Serializable]
    public struct ActionsInFrame : NetInfo
    {
        public List<PlayerAction> actionsInOneFrame;
        public float frameDeltaTime;

        public ActionsInFrame(List<PlayerAction> actionsInOneFrame, float frameDeltaTime)
        {
            this.actionsInOneFrame = new List<PlayerAction>(actionsInOneFrame);
            this.frameDeltaTime = frameDeltaTime;
        }

        public byte[] Serialize()
        {
            List<byte[]> dataToAdd = new List<byte[]>();
            int size = 0;

            //Amount of actions
            dataToAdd.Add(BitConverter.GetBytes((short)actionsInOneFrame.Count));
            size += 2;

            //The actions themselves preceded by their size
            foreach (var action in actionsInOneFrame)
            {
                byte[] serializedAction = action.Serialize();
                byte[] actionSize = BitConverter.GetBytes((short)serializedAction.Length);

                dataToAdd.Add(actionSize);
                dataToAdd.Add(serializedAction);

                size += 2 + serializedAction.Length;
            }

            //The deltaTime
            dataToAdd.Add(BitConverter.GetBytes(frameDeltaTime));
            size += 4;

            //Put all of it in the resulting byte[]
            byte[] result = new byte[size];
            size = 0;

            foreach (var item in dataToAdd)
            {
                Buffer.BlockCopy(item, 0, result, size, item.Length);
                size += item.Length;
            }

            return result;
        }
        public void Deserialize(byte[] data)
        {
            actionsInOneFrame = new List<PlayerAction>();

            short actionAmount = BitConverter.ToInt16(data, 0);

            int offset = 2;

            for (int i = 0; i < actionAmount; i++)
            {
                short actionSize = BitConverter.ToInt16(data, offset);
                offset += 2;

                byte[] actionToDecode = new byte[actionSize];
                Buffer.BlockCopy(data, offset, actionToDecode, 0, actionSize);
                offset += actionSize;

                PlayerAction actionToAdd = new PlayerAction();
                actionToAdd.Deserialize(actionToDecode);
                actionsInOneFrame.Add(actionToAdd);
            }

            frameDeltaTime = BitConverter.ToSingle(data, offset);
        }
    }

    [Serializable]
    public struct PlayerActionList : NetInfo
    {
        public PlayerActionList(uint playerID, List<ActionsInFrame> frameList)
        {
            id = playerID;
            l = new List<ActionsInFrame>(frameList);
        }

        public uint id; //Id of the player
        public List<ActionsInFrame> l; //List of actions

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
                l = new List<ActionsInFrame>();

                short actionSize = BitConverter.ToInt16(data, offset);
                offset += 2;

                byte[] actionToDecode = new byte[actionSize];
                Buffer.BlockCopy(data, offset, actionToDecode, 0, actionSize);

                ActionsInFrame actionsToAdd = new ActionsInFrame();

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
            camPivot = Quaternion.identity;
        }
        public Player(PlayerBehaviour instance)
        {
            id = instance.netID;
            o = instance.ownerName;
            camPivot = instance.camPivot.transform.rotation;
        }

        // Shortened names equals more space to add in buffer
        public uint id;
        public string o; //Owner name
        public Quaternion camPivot; //Rotation of the cam pivot

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(id);
            writer.Write(o);
            
            writer.Write(camPivot.x);
            writer.Write(camPivot.y);
            writer.Write(camPivot.z);
            writer.Write(camPivot.w);

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
            
            camPivot.x = reader.ReadSingle();
            camPivot.y = reader.ReadSingle();
            camPivot.z = reader.ReadSingle();
            camPivot.w = reader.ReadSingle();

            stream.Close();
        }
    }

    [Serializable]
    public struct BasicZombie : NetInfo
    {
        public BasicEnemy.State currentState;

        public BasicZombie(BasicEnemy instance)
        {
            currentState = instance.currentState;
        }

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((char)currentState);

            byte[] data = stream.ToArray();

            stream.Close();
            writer.Close();

            return data;
        }

        public void Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            currentState = (BasicEnemy.State)reader.ReadChar();

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
        public Vector3 position;
        public Quaternion rotation;

        public NetObjInfo(uint id, NetInfo objectInfo, Vector3 pos, Quaternion rot)
        {
            this.id = id;
            this.objectInfo = objectInfo;
            position = pos;
            rotation = rot;
        }

        public byte[] Serialize()
        {
            byte[] serializedID = BitConverter.GetBytes(id);


            byte[] serializedType = new byte[1]; 
            serializedType[0] = (byte)Types.encodeTypes[objectInfo.GetType()];


            byte[] serializedClass = objectInfo.Serialize();


            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(position.x);
            writer.Write(position.y);
            writer.Write(position.z);

            writer.Write(rotation.x);
            writer.Write(rotation.y);
            writer.Write(rotation.z);
            writer.Write(rotation.w);

            byte[] transformData = stream.ToArray();

            stream.Close();
            writer.Close();


            byte[] result = new byte[serializedID.Length + 1 + serializedClass.Length + transformData.Length];

            Buffer.BlockCopy(serializedID, 0, result, 0, serializedID.Length);
            Buffer.BlockCopy(serializedType, 0, result, serializedID.Length, 1);
            Buffer.BlockCopy(serializedClass, 0, result, serializedID.Length + 1, serializedClass.Length);
            Buffer.BlockCopy(transformData, 0, result, serializedID.Length + 1 + serializedClass.Length, transformData.Length);

            return result;
        }

        public void Deserialize(byte[] data)
        {
            id = BitConverter.ToUInt32(data, 0);


            Types.ClassIdentifyers classType = (Types.ClassIdentifyers)data[4];


            byte[] classToDecode = new byte[data.Length - 5 - (sizeof(float) * 7)]; //4 from the uint and 1 from the type and plus transform data
            Buffer.BlockCopy(data, 5, classToDecode, 0, data.Length - 5 - (sizeof(float) * 7));


            NetInfo classInstance = (NetInfo)Activator.CreateInstance(Types.decodeTypes[classType]);
            classInstance.Deserialize(classToDecode);

            objectInfo = classInstance;


            byte[] transformData = new byte[sizeof(float) * 7];
            Buffer.BlockCopy(data, 5 + classToDecode.Length, transformData, 0, sizeof(float) * 7);

            MemoryStream stream = new MemoryStream(transformData);
            BinaryReader reader = new BinaryReader(stream);

            position.x = reader.ReadSingle();
            position.y = reader.ReadSingle();
            position.z = reader.ReadSingle();

            rotation.x = reader.ReadSingle();
            rotation.y = reader.ReadSingle();
            rotation.z = reader.ReadSingle();
            rotation.w = reader.ReadSingle();

            stream.Close();
        }
    }
}