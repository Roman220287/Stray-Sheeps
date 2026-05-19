using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UpgradeOption
{
    public string title;
    [TextArea] public string description;
}

public class ChooseUpgradeMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject upgradeMenuUI;
    public Button[] optionButtons;
    public Text[] optionTitles;
    public Text[] optionDescriptions;
    public Behaviour[] disableWhileMenuOpen;

    [Header("Upgrade Options")]
    [Tooltip("All upgrades that can be offered when a wave is cleared.")]
    public List<UpgradeOption> allUpgrades = new List<UpgradeOption>();

    [Tooltip("Number of choices shown to the player each wave.")]
    public int choicesToShow = 3;

    private List<int> activeChoices = new List<int>();

    private void Awake()
    {
        if (upgradeMenuUI != null)
            upgradeMenuUI.SetActive(false);
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
    }

    public void ChooseOption(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= activeChoices.Count)
            return;

        UpgradeOption chosen = allUpgrades[activeChoices[buttonIndex]];
        Debug.Log($"Upgrade selected: {chosen.title}");
        CloseMenu();

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
