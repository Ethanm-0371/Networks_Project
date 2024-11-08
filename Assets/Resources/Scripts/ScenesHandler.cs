using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesHandler : MonoBehaviour
{
    public static ScenesHandler Singleton { get; private set; }

    private AsyncOperation sceneLoading;

    [SerializeField] Object loadingScreenPrefab;
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

        sceneLoading = SceneManager.LoadSceneAsync(sceneName, loadMode);
        sceneLoading.completed += OnSceneLoaded;
    }

    void OnSceneLoaded(AsyncOperation loadingFunction)
    {
        sceneLoading = null;

        Destroy(loadingScreenGO);
        Debug.Log("New Scene Loaded");
    }
}
