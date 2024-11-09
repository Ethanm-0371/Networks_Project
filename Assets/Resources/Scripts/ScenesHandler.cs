using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesHandler : MonoBehaviour
{
    public static ScenesHandler Singleton { get; private set; }

    [SerializeField] UnityEngine.Object loadingScreenPrefab;
    GameObject loadingScreenGO;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Singleton != null && Singleton != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Singleton = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadScene(string sceneName, LoadSceneMode loadMode)
    {
        loadingScreenGO = (GameObject)Instantiate(loadingScreenPrefab);
        DontDestroyOnLoad(loadingScreenGO);

        //sceneLoading = SceneManager.LoadSceneAsync(sceneName, loadMode);
        //sceneLoading.completed += (AsyncOperation func) => { PacketHandler.SendPacket(GameClient.Singleton.clientSocket, GameClient.Singleton.serverEndPoint, new SceneLoadedData()); };

        SceneManager.LoadSceneAsync(sceneName, loadMode).completed += (AsyncOperation func) => { PacketHandler.SendPacket(GameClient.Singleton.clientSocket, GameClient.Singleton.serverEndPoint, new SceneLoadedData()); };
    }

    public void SetReady()
    {
        Destroy(loadingScreenGO);
    }
}
