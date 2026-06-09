using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Controller Navigation")]
    public float menuMoveDeadzone = 0.5f;
    public float menuRepeatDelay = 0.25f;
    public float menuRepeatRate = 0.12f;

    private InputSystem_Actions controls;
    private bool isPaused;
    private Vector2 navigateInput;
    private bool hasMoveInput;
    private float nextMoveTime;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        controls.UI.Cancel.performed += OnPauseInput;
        controls.UI.Submit.performed += OnSubmit;
        controls.UI.Navigate.performed += OnNavigate;
        controls.UI.Navigate.canceled += OnNavigateCanceled;
    }

    private void OnEnable()
    {
        controls?.UI.Enable();
        SetPause(false);
    }

    private void OnDisable()
    {
        controls?.UI.Disable();
    }

    private void OnDestroy()
    {
        controls?.Dispose();
    }

    private void Update()
    {
        if (EventSystem.current == null)
            return;

        if (pauseMenuUI != null && pauseMenuUI.activeInHierarchy)
        {
            MaintainSelection(firstSelectedWhenPaused);
            HandleNavigation();
        }
        else if (titleScreenUI != null && titleScreenUI.activeInHierarchy)
        {
            MaintainSelection(firstSelectedOnTitle);
            HandleNavigation();
        }
        else if (creditsScreenUI != null && creditsScreenUI.activeInHierarchy)
        {
            MaintainSelection(firstSelectedOnTitle);
            HandleNavigation();
        }
    }

    private void OnPauseInput(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.performed || EventSystem.current == null)
            return;

        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
            return;

        ExecuteEvents.Execute(selected, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        navigateInput = context.ReadValue<Vector2>();
    }

    private void OnNavigateCanceled(InputAction.CallbackContext context)
    {
        navigateInput = Vector2.zero;
        hasMoveInput = false;
        nextMoveTime = Time.unscaledTime;
    }

    private void HandleNavigation()
    {
        if (navigateInput.sqrMagnitude < menuMoveDeadzone * menuMoveDeadzone)
        {
            hasMoveInput = false;
            nextMoveTime = Time.unscaledTime;
            return;
        }

        if (Time.unscaledTime < nextMoveTime)
            return;

        MoveSelection(navigateInput);
        nextMoveTime = Time.unscaledTime + (hasMoveInput ? menuRepeatRate : menuRepeatDelay);
        hasMoveInput = true;
    }

    private void MaintainSelection(GameObject firstSelected)
    {
        if (firstSelected == null)
            return;

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }

    private void MoveSelection(Vector2 input)
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current == null)
            return;

        var selectable = current.GetComponent<Selectable>();
        if (selectable == null)
            return;

        Selectable next = null;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            next = input.x > 0 ? selectable.FindSelectableOnRight() : selectable.FindSelectableOnLeft();
        else
            next = input.y > 0 ? selectable.FindSelectableOnUp() : selectable.FindSelectableOnDown();

        if (next != null)
            EventSystem.current.SetSelectedGameObject(next.gameObject);
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
        Cursor.lockState = CursorLockMode.None;
    }

    public void Pause()
    {
        SetPause(true);
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("Title Screen");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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