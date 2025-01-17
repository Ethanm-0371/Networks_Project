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

    bool loadingScreenReady = false;

    private void Awake()
    {
        #region Singleton

        if (Singleton != null && Singleton != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Singleton = this;
        }

        DontDestroyOnLoad(this.gameObject);

        #endregion
    }

    private void Update()
    {
        if (loadingScreenReady)
        {
            Destroy(loadingScreenGO);
            loadingScreenReady = false;
        }
    }

    public void LoadScene(string sceneName, LoadSceneMode loadMode)
    {
        loadingScreenGO = (GameObject)Instantiate(loadingScreenPrefab);
        DontDestroyOnLoad(loadingScreenGO);

        SceneManager.LoadSceneAsync(sceneName, loadMode).completed += (AsyncOperation func) =>
        {
            if (GameClient.Singleton != null)
            {
                PacketHandler.SendPacket(GameClient.Singleton.clientSocket, GameClient.Singleton.serverEndPoint, PacketType.SceneLoadedFlag, new Wrappers.SceneLoadedData(1));
            }
            else
            {
                Debug.LogWarning("Could not load scenes because Client not instantiated.");
            }
        };
    }

    public void SetReady()
    {
        loadingScreenReady = true;
    }
}
