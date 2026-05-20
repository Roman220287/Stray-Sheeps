using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenuUI;
    public GameObject firstSelectedWhenPaused;
    public Behaviour[] disableWhilePaused;

    [Header("Title Screen")]
    public GameObject titleScreenUI;
    public GameObject firstSelectedOnTitle;
    public GameObject creditsScreenUI;

    private InputSystem_Actions controls;
    private bool isPaused;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        controls.UI.Cancel.performed += OnPauseInput;
    }

    private void OnEnable()
    {
        controls.UI.Enable();
        SetPause(false);
    }

    private void OnDisable()
    {
        controls.UI.Disable();
    }

    private void OnDestroy()
    {
        controls.Dispose();
    }

    private void OnPauseInput(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        SetPause(!isPaused);
    }

    public void SetPause(bool pause)
    {
        isPaused = pause;
        pauseMenuUI.SetActive(pause);
        Time.timeScale = pause ? 0f : 1f;
        Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.None;
        Cursor.visible = !pause;

        if (disableWhilePaused != null)
        {
            foreach (var behaviour in disableWhilePaused)
            {
                if (behaviour != null)
                    behaviour.enabled = !pause;
            }
        }

        if (pause && firstSelectedWhenPaused != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedWhenPaused);
        }
    }

    public void Resume()
    {
        SetPause(false);
    }

    public void Pause()
    {
        SetPause(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

     //only for title screen
     #region Title Screen
    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void ShowCredits()
    {
        titleScreenUI.SetActive(false);
        creditsScreenUI.SetActive(true);
        if (firstSelectedOnTitle != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedOnTitle);
        }
    }

    public void BackToTitle()
    {
        creditsScreenUI.SetActive(false);
        titleScreenUI.SetActive(true);
        if (firstSelectedOnTitle != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedOnTitle);
        }
    }
    #endregion
}