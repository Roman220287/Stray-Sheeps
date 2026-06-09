using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class UpgradeOption
{
    public string title;
    [TextArea] public string description;
    public PickUpBase.StatType statToModify;
    [Tooltip("The percentage to increase the stat by when this upgrade is chosen.")]
    public float percentageIncrease = 10f;
}

public class ChooseUpgradeMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject upgradeMenuUI;
    public Button[] optionButtons;
    public Text[] optionTitles;
    public Text[] optionDescriptions;
    public Behaviour[] disableWhileMenuOpen;
    public PickUpBase statsToApply;

    [Header("Win Screen")]
    public GameObject winScreenUI;
    [Tooltip("How long to show the win screen after choosing an upgrade (in seconds)")]
    public float winScreenDuration = 3f;

    [Header("Upgrade Options")]
    [Tooltip("All upgrades that can be offered when a wave is cleared.")]
    public List<UpgradeOption> allUpgrades = new List<UpgradeOption>();

    [Tooltip("Number of choices shown to the player each wave.")]
    public int choicesToShow = 3;

    private List<int> activeChoices = new List<int>();
    private InputSystem_Actions controls;
    private Vector2 navigateInput;
    private Vector2 previousNavigateInput;
    private float nextMoveTime;

    private void Awake()
    {
        if (upgradeMenuUI != null)
            upgradeMenuUI.SetActive(false);

        controls = new InputSystem_Actions();
        controls.UI.Navigate.performed += OnNavigate;
        controls.UI.Navigate.canceled += OnNavigateCanceled;
    }

    private void OnEnable()
    {
        controls?.UI.Enable();
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
        if (EventSystem.current == null || upgradeMenuUI == null || !upgradeMenuUI.activeInHierarchy)
            return;

        HandleNavigation();
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        navigateInput = context.ReadValue<Vector2>();
    }

    private void OnNavigateCanceled(InputAction.CallbackContext context)
    {
        navigateInput = Vector2.zero;
        previousNavigateInput = Vector2.zero;
        nextMoveTime = Time.unscaledTime;
    }

    private void HandleNavigation()
    {
        float deadzone = 0.5f;
        if (navigateInput.sqrMagnitude < deadzone * deadzone)
        {
            previousNavigateInput = Vector2.zero;
            return;
        }

        // Check if the direction has changed
        bool directionChanged = Vector2.Distance(navigateInput.normalized, previousNavigateInput.normalized) > 0.1f;

        if (directionChanged)
        {
            MoveSelection(navigateInput);
            previousNavigateInput = navigateInput.normalized;
            nextMoveTime = Time.unscaledTime + 0.25f;
        }
        else if (Time.unscaledTime >= nextMoveTime)
        {
            MoveSelection(navigateInput);
            nextMoveTime = Time.unscaledTime + 0.12f;
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

    public void ShowMenu()
    {
        PopulateOptions();

        if (disableWhileMenuOpen != null)
        {
            foreach (var behaviour in disableWhileMenuOpen)
            {
                if (behaviour != null)
                    behaviour.enabled = false;
            }
        }

        if (upgradeMenuUI != null)
            upgradeMenuUI.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMenu()
    {
        if (disableWhileMenuOpen != null)
        {
            foreach (var behaviour in disableWhileMenuOpen)
            {
                if (behaviour != null)
                    behaviour.enabled = true;
            }
        }

        if (upgradeMenuUI != null)
            upgradeMenuUI.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void PopulateOptions()
    {
        activeChoices.Clear();
        if (allUpgrades == null || allUpgrades.Count == 0)
        {
            Debug.LogWarning("ChooseUpgradeMenu: No upgrades configured.");
            return;
        }

        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < allUpgrades.Count; i++)
            availableIndexes.Add(i);

        Shuffle(availableIndexes);

        int offerCount = Mathf.Min(choicesToShow, availableIndexes.Count);
        for (int i = 0; i < offerCount; i++)
            activeChoices.Add(availableIndexes[i]);

        if (optionButtons == null)
            return;

        for (int slot = 0; slot < optionButtons.Length; slot++)
        {
            bool hasOption = slot < activeChoices.Count;
            if (optionButtons[slot] != null)
                optionButtons[slot].gameObject.SetActive(hasOption);

            if (hasOption)
            {
                UpgradeOption option = allUpgrades[activeChoices[slot]];
                if (optionTitles != null && slot < optionTitles.Length && optionTitles[slot] != null)
                    optionTitles[slot].text = option.title;

                if (optionDescriptions != null && slot < optionDescriptions.Length && optionDescriptions[slot] != null)
                    optionDescriptions[slot].text = option.description;

                int buttonIndex = slot;
                optionButtons[slot].onClick.RemoveAllListeners();
                optionButtons[slot].onClick.AddListener(() => ChooseOption(buttonIndex));
            }
            else
            {
                if (optionButtons[slot] != null)
                    optionButtons[slot].onClick.RemoveAllListeners();
            }
        }

        // Set the first button as selected for controller navigation
        if (optionButtons.Length > 0 && optionButtons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionButtons[0].gameObject);
        }
    }

    public void ChooseOption(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= activeChoices.Count)
            return;

        UpgradeOption chosen = allUpgrades[activeChoices[buttonIndex]];
        Debug.Log($"Upgrade selected: {chosen.title}");

        ApplyUpgrade(chosen);
        CloseMenu();

        StartCoroutine(ShowWinScreenThenProceed());
    }

    private void ApplyUpgrade(UpgradeOption option)
    {
        if (statsToApply == null)
        {
            Debug.LogWarning("ChooseUpgradeMenu: statsToApply is not assigned. Cannot apply upgrade.");
            return;
        }

        PlayerStatsBase playerStats = FindObjectsByType<PlayerStatsBase>(FindObjectsSortMode.None)[0];
        if (playerStats == null)
        {
            Debug.LogWarning("ChooseUpgradeMenu: No PlayerStatsBase found in scene. Cannot apply upgrade.");
            return;
        }

        statsToApply.statToModify = option.statToModify;
        statsToApply.percentageIncrease = option.percentageIncrease;
        statsToApply.ApplyPickupTo(playerStats);
    }

    private IEnumerator ShowWinScreenThenProceed()
    {
        if (winScreenUI != null)
            winScreenUI.SetActive(true);

        yield return new WaitForSecondsRealtime(winScreenDuration);

        if (winScreenUI != null)
            winScreenUI.SetActive(false);

        if (NextLevelManager.instance != null)
            NextLevelManager.instance.ProceedToNextLevel();
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
