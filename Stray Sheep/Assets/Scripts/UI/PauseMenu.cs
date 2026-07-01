using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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

    [Header("Background Blur")]
    [SerializeField] private GameObject backgroundBlurOverlay;

    [Header("Background Scrolling Shader")]
    [SerializeField] private GameObject backgroundScrollOverlay;

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
    private readonly List<Behaviour> pausedBehaviours = new();
    private readonly List<NavMeshAgent> pausedAgents = new();
    private readonly List<Rigidbody> pausedRigidbodies = new();
    private readonly List<bool> pausedAgentWasStopped = new();
    private readonly List<bool> pausedRigidbodyWasKinematic = new();

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
        PauseManager.SetPaused(pause);

        if (pause)
            DisableGameplayLogic();
        else
        {
            RestoreGameplayLogic();
            ReenablePlayerInput();
        }

        EnsureScrollingOverlay();

        if (backgroundBlurOverlay != null)
            backgroundBlurOverlay.SetActive(pause);

        if (backgroundScrollOverlay != null)
            backgroundScrollOverlay.SetActive(pause);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(pause);

        // Keep time running so shader-based background motion keeps animating.

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

    private void DisableGameplayLogic()
    {
        pausedBehaviours.Clear();
        pausedAgents.Clear();
        pausedRigidbodies.Clear();
        pausedAgentWasStopped.Clear();
        pausedRigidbodyWasKinematic.Clear();

        foreach (var behaviour in FindObjectsByType<Behaviour>(FindObjectsSortMode.None))
        {
            if (behaviour == this || behaviour == null)
                continue;

            if (behaviour is PauseMenu)
                continue;

            if (behaviour is PlayerBase)
            {
                continue;
            }

            if (behaviour is EnemyBase || behaviour is WaveSpawner || behaviour is SpawnDropEffect || behaviour is AttackInstance)
            {
                pausedBehaviours.Add(behaviour);
                behaviour.enabled = false;
            }
        }

        foreach (var agent in FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None))
        {
            if (agent == null)
                continue;

            pausedAgents.Add(agent);
            pausedAgentWasStopped.Add(agent.isStopped);
            agent.isStopped = true;
        }

        foreach (var rb in FindObjectsByType<Rigidbody>(FindObjectsSortMode.None))
        {
            if (rb == null)
                continue;

            pausedRigidbodies.Add(rb);
            pausedRigidbodyWasKinematic.Add(rb.isKinematic);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.Sleep();
        }

        if (disableWhilePaused != null)
        {
            foreach (var behaviour in disableWhilePaused)
            {
                if (behaviour != null && behaviour.enabled)
                {
                    pausedBehaviours.Add(behaviour);
                    behaviour.enabled = false;
                }
            }
        }
    }

    private void ReenablePlayerInput()
    {
        var players = FindObjectsByType<PlayerBase>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player != null)
                player.ReenableInput();
        }
    }

    private void RestoreGameplayLogic()
    {
        for (int i = 0; i < pausedBehaviours.Count; i++)
        {
            if (pausedBehaviours[i] != null)
                pausedBehaviours[i].enabled = true;
        }

        for (int i = 0; i < pausedAgents.Count; i++)
        {
            if (pausedAgents[i] != null)
                pausedAgents[i].isStopped = pausedAgentWasStopped[i];
        }

        for (int i = 0; i < pausedRigidbodies.Count; i++)
        {
            if (pausedRigidbodies[i] != null)
            {
                pausedRigidbodies[i].isKinematic = pausedRigidbodyWasKinematic[i];
                pausedRigidbodies[i].WakeUp();
            }
        }

        pausedBehaviours.Clear();
        pausedAgents.Clear();
        pausedRigidbodies.Clear();
        pausedAgentWasStopped.Clear();
        pausedRigidbodyWasKinematic.Clear();
    }

    private void EnsureScrollingOverlay()
    {
        if (backgroundScrollOverlay != null)
            return;

        if (pauseMenuUI == null)
            return;

        var canvas = pauseMenuUI.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

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
        
        PauseManager.SetPaused(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title Screen");
    }

     //only for title screen
     #region Title Screen
    public void StartGame()
    {
        PauseManager.SetPaused(false);
        Time.timeScale = 1f;
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