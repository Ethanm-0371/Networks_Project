using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScreen : MonoBehaviour
{
    [SerializeField] GameObject canvas;

    bool isMenuActive = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        isMenuActive = !isMenuActive;
        canvas.SetActive(isMenuActive);
        UpdateCursorState(isMenuActive);
        GameClient.Singleton.ownedPlayerGO.GetComponentInChildren<PlayerBehaviour>().lockCamera = isMenuActive;
    }

    private void UpdateCursorState(bool isMenuOpen)
    {
        Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isMenuOpen;
    }

    public void OnClickContinue()
    {
        ToggleMenu();
    }

    public void OnClickExitServer()
    {
        if (GameServer.Singleton != null)
        {
            Destroy(GameServer.Singleton);
        }
        if (GameClient.Singleton != null)
        {
            Destroy(GameClient.Singleton);
        }

        ScenesHandler.Singleton.LoadScene("Main_Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnClickExitToDesktop()
    {
        if (GameServer.Singleton != null)
        {
            Destroy(GameServer.Singleton);
        }
        if (GameClient.Singleton != null)
        {
            Destroy(GameClient.Singleton);
        }

        Application.Quit();
    }
}
