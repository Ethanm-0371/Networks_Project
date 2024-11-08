using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public PlayerData(string username)
    {
        _username = username;
    }

    [SerializeField] private string _username;

    public string Username => _username;
}
