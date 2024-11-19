using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SerializationPlayground))]
public class SerializationPlaygroundEditor : Editor
{
    SerializationPlayground myTarget;

    int uintConversion = 0;
    Vector4 quaternionConversion = Vector4.zero;

    object classToSerialize = null;
    Wrappers.Player playerToEncode = new Wrappers.Player();


    public override void OnInspectorGUI()
    {
        myTarget = (SerializationPlayground)target;

        ShowSerializationTypes();

        GUILayout.Space(10);

        ShowClassSerialization();

        GUILayout.Space(10);

        ShowPrintButtons();
    }

    void ShowSerializationTypes()
    {
        EditorGUILayout.LabelField("SerializationTypes", EditorStyles.boldLabel);

        myTarget.encodingType = (SerializationPlayground.EncodingType)EditorGUILayout.EnumPopup("Type of serialization: ", myTarget.encodingType);
        myTarget.type = (PacketType)EditorGUILayout.EnumPopup("Enum to serialize: ", myTarget.type);
    }
    void ShowClassSerialization()
    {
        EditorGUILayout.LabelField("Class to Serialize", EditorStyles.boldLabel);

        switch (myTarget.type)
        {
            case PacketType.PlayerData:
                PlayerSerialization();

                playerToEncode.id = (uint)uintConversion;
                playerToEncode.r.x = quaternionConversion.x;
                playerToEncode.r.y = quaternionConversion.y;
                playerToEncode.r.z = quaternionConversion.z;
                playerToEncode.r.w = quaternionConversion.w;

                classToSerialize = playerToEncode;
                break;
            case PacketType.AssignOwnership:
                break;
            case PacketType.SceneLoadedFlag:
                break;
            case PacketType.netObjsDictionary:
                break;
            case PacketType.playerActionsList:
                break;
            case PacketType.None:
                EditorGUILayout.LabelField("No data class selected");
                break;
            default:
                break;
        }
    }
    void ShowPrintButtons()
    {
        EditorGUILayout.LabelField("Printing", EditorStyles.boldLabel);

        //Print one
        if (GUILayout.Button("Print one"))
        {
            Debug.Log($"1 {myTarget.type} in {myTarget.encodingType} occupies {myTarget.ClassEncoder(1, classToSerialize).Length} bytes");
        }

        //Print multiple
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Print multiple"))
        {
            Debug.Log($"{myTarget.printAmount} {myTarget.type} in {myTarget.encodingType} occupy {myTarget.ClassEncoder(myTarget.printAmount, classToSerialize).Length} bytes");
        }
        myTarget.printAmount = EditorGUILayout.IntField(myTarget.printAmount);

        GUILayout.EndHorizontal();
    }

    void PlayerSerialization()
    {
        uintConversion = EditorGUILayout.IntField("ID", uintConversion);
        playerToEncode.o = EditorGUILayout.TextField("Owner", playerToEncode.o);
        playerToEncode.p = EditorGUILayout.Vector3Field("Position", playerToEncode.p);
        quaternionConversion = EditorGUILayout.Vector4Field("Rotation", quaternionConversion);
    }
}
