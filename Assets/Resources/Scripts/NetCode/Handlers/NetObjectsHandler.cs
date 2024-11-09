using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseWrapper
{
    public int id;
}

[Serializable] public class TestWrapper : BaseWrapper
{
    public TestWrapper(int instance)
    {
        mondongo = instance;
    }

    public int mondongo;
}
[Serializable] public class TestWrapper2 : BaseWrapper
{
    public TestWrapper2(int instance)
    {
        mondongo2 = instance;
    }

    public int mondongo2;
}

[Serializable]
public class DictWrapper2
{
    public DictWrapper2(Dictionary<int, string> dict, int num, List<BaseWrapper> intlist)
    {
        dictionary = dict;
        myNum = num;
        myintlist = intlist;
    }

    public Dictionary<int, string> dictionary;
    public int myNum;
    public List<BaseWrapper> myintlist;
}

public class NetObjectsHandler : MonoBehaviour
{
    enum WrapperTypes
    {
        None,
        Player,
        Test
    }

    Dictionary<Type, string> prefabPaths = new Dictionary<Type, string>()
    {
        { typeof(object),        "" },
        { typeof(PlayerWrapper), "PlayerPrefab" },
    };

    public Dictionary<int, object> netObjects = new Dictionary<int, object>();

    Dictionary<int, string> dick = new Dictionary<int, string>();

    private void Awake()
    {
        TestWrapper elem1 = new TestWrapper(3);
        TestWrapper2 elem2 = new TestWrapper2(4);

        //netObjects.Add(123456, elem1);
        //netObjects.Add(696969, elem2);

        dick.Add(23, "Mondongo");
        dick.Add(69, "Bobo");

        List<BaseWrapper> mon = new List<BaseWrapper>() { elem1, elem2 };

        DictWrapper2 elem3 = new DictWrapper2(dick, 69, mon);

        string json = JsonUtility.ToJson(elem3);

        Debug.Log(json);
    }

    List<object> SerializeDict(Dictionary<int, object> netObjs)
    {
        List<object> objsList = new List<object>();

        foreach (var item in netObjects)
        {
            objsList.Add(item);
        }

        return objsList;
    }

    void InstantiateNetObjects(/*list*/)
    {
        //foreach (var item in /*list*/)
        //{
        //    Instantiate(Resources.Load("Prefabs/" + prefabPaths[/*PrefabTypes*/]));
        //}
    }
}
