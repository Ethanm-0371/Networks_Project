using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScreen : MonoBehaviour
{
    [SerializeField] GameObject canvas;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            canvas.SetActive(!canvas.activeSelf);

            if (canvas.activeSelf) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Locked;

        }
    }

    public void OnClickContinue()
    {
        canvas.SetActive(!canvas.activeSelf);
    }

    public void OnClickExitServer()
    {
        if (GameServer.Singleton != null)
        {
            Destroy(GameServer.Singleton);
        }
        Destroy(GameClient.Singleton);

        ScenesHandler.Singleton.LoadScene("Main_Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnClickExitToDesktop()
    {
        if (GameServer.Singleton != null)
        {
            Destroy(GameServer.Singleton);
        }
        Destroy(GameClient.Singleton);

        Application.Quit();
    }
}
