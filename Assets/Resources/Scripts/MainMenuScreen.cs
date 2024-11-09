using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScreen : MonoBehaviour
{
    [SerializeField] Object serverPrefab;
    [SerializeField] Object clientPrefab;

    [SerializeField] TMP_InputField IPInputField;
    [SerializeField] TMP_InputField HostUsernameInputField;
    [SerializeField] TMP_InputField ClientUsernameInputField;

    public void OnClickHost()
    {
        if (string.IsNullOrWhiteSpace(HostUsernameInputField.text)) { return; } //Handle error

        GameObject serverGO = (GameObject)Instantiate(serverPrefab);
        serverGO.GetComponent<GameServer>().Init();
        serverGO.GetComponent<GameClient>().Init(IPAddress.Loopback.ToString(), HostUsernameInputField.text);

        ScenesHandler.Singleton.LoadScene("Lobby", LoadSceneMode.Single);
    }
    public void OnClickJoin()
    {
        if (string.IsNullOrWhiteSpace(IPInputField.text) || 
            string.IsNullOrWhiteSpace(ClientUsernameInputField.text)) { return; } //Handle error

        //Send ping to check server exists

        //If server exists, connect
        GameObject clientGO = (GameObject) Instantiate(clientPrefab);
        clientGO.GetComponent<GameClient>().Init(IPInputField.text, ClientUsernameInputField.text);

        ScenesHandler.Singleton.LoadScene("Lobby", LoadSceneMode.Single);
    }
    public void OnClickExit()
    {
        Application.Quit();
    }
}
