using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class NextLevelManager : MonoBehaviour
{
    public static NextLevelManager instance;
    public static int CurrentDepth { get; private set; }

    public static NextLevelManager ResolveInstance()
    {
        if (instance != null)
            return instance;

        instance = FindFirstObjectByType<NextLevelManager>(FindObjectsInactive.Include);
        if (instance != null)
            Debug.Log("NextLevelManager: Recovered instance via FindFirstObjectByType.");
        return instance;
    }

    [Header("Layout Progression")]
    [SerializeField] private Transform[] layoutAnchors;
    [SerializeField] private float layoutTransitionDelay = 0.5f;

    public ChooseUpgradeMenu upgradeMenu;

    private int enemiesAlive;
    private bool allWavesComplete = false;
    private bool levelEnding = false;
    [SerializeField] public int depth = 0;

    private static readonly System.Collections.Generic.List<StoredUpgrade> staticStoredUpgrades = new System.Collections.Generic.List<StoredUpgrade>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        depth = CurrentDepth;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (layoutAnchors == null || layoutAnchors.Length == 0)
        {
            layoutAnchors = FindObjectsByType<LevelLayoutAnchor>(FindObjectsSortMode.None)
                .OrderBy(x => x.layoutIndex)
                .Select(x => x.transform)
                .ToArray();
        }
    }

    private struct StoredUpgrade
    {
        public PickUpBase.StatType stat;
        public float percentage;
    }

    public void RecordUpgrade(PickUpBase.StatType stat, float percentage)
    {
        staticStoredUpgrades.Add(new StoredUpgrade { stat = stat, percentage = percentage });
        Debug.Log($"NextLevelManager: Recorded upgrade {stat} {percentage}%.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
            return;
        }
        enemiesAlive = 0;
        allWavesComplete = false;
        levelEnding = false;

        // FORCE clear any lingering system pause states before doing anything else
        Time.timeScale = 1f;
        PauseManager.SetPaused(false); 

        // Re-find the upgrade menu instance in the newly loaded scene
        upgradeMenu = ResolveUpgradeMenu();
        Debug.Log($"NextLevelManager: Scene loaded {scene.name} (index {scene.buildIndex}); upgrade menu resolved: {(upgradeMenu != null ? "yes" : "no")}");

        StartCoroutine(ApplyStoredUpgradesAfterLoad(scene));
    }

    private IEnumerator ApplyStoredUpgradesAfterLoad(UnityEngine.SceneManagement.Scene scene)
    {
        yield return null;

        int attempts = 0;
        PlayerStatsBase player = null;
        while (attempts < 20)
        {
            player = FindPlayerInScene(scene);
            if (player != null)
                break;

            attempts++;
            yield return new WaitForSecondsRealtime(0.05f);
        }

        if (player == null)
        {
            Debug.LogWarning("NextLevelManager: No PlayerStatsBase found in loaded scene. Stored upgrades were not applied.");
            yield break;
        }

        ApplyStoredUpgradesToPlayer(player);
    }

    private PlayerStatsBase FindPlayerInScene(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (GameObject rootGameObject in scene.GetRootGameObjects())
        {
            PlayerStatsBase player = rootGameObject.GetComponentInChildren<PlayerStatsBase>(true);
            if (player != null)
                return player;
        }
        return null;
    }

    private void ApplyStoredUpgradesToPlayer(PlayerStatsBase player)
    {
        if (staticStoredUpgrades.Count == 0) return;

        foreach (var su in staticStoredUpgrades)
        {
            GameObject temp = new GameObject("_TempPickup");
            var pickup = temp.AddComponent<PickUpBase>();
            pickup.statToModify = su.stat;
            pickup.percentageIncrease = su.percentage;
            pickup.ApplyPickupTo(player);
            Destroy(temp);
        }

        Debug.Log($"NextLevelManager: Applied {staticStoredUpgrades.Count} stored upgrade(s) to player.");
        staticStoredUpgrades.Clear();
    }

    public void RegisterEnemy()
    {
        enemiesAlive += 1;
        Debug.Log($"NextLevelManager: Enemy registered. Alive count: {enemiesAlive}");
    }

    public void UnregisterEnemy()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        Debug.Log($"NextLevelManager: Enemy unregistered. Alive count: {enemiesAlive}");
        CheckLevelComplete();
    }

    public void WavesFinished()
    {
        allWavesComplete = true;
        Debug.Log("NextLevelManager: All waves finished. Checking win condition.");
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        if (!levelEnding && allWavesComplete && enemiesAlive <= 0)
        {
            levelEnding = true;
            ChooseUpgradeMenu menu = ResolveUpgradeMenu();
            Debug.Log($"NextLevelManager: Level complete; attempting to show upgrade UI. Menu found: {(menu != null ? "yes" : "no")}");
            if (menu != null)
            {
                menu.ShowMenu();
                Debug.Log("NextLevelManager: ShowMenu() called on upgrade menu.");
            }
            else
            {
                Debug.LogWarning("NextLevelManager: Upgrade menu not found; advancing without showing upgrade UI.");
                StartCoroutine(LoadNextLevel());
            }
        }
    }

    private ChooseUpgradeMenu ResolveUpgradeMenu()
    {
        if (upgradeMenu != null)
            return upgradeMenu;

        upgradeMenu = FindFirstObjectByType<ChooseUpgradeMenu>(FindObjectsInactive.Include);
        Debug.Log($"NextLevelManager: ResolveUpgradeMenu() found menu: {(upgradeMenu != null ? "yes" : "no")}");
        return upgradeMenu;
    }

    public void ProceedToNextLevel()
    {
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(layoutTransitionDelay);

        CurrentDepth = depth + 1;
        depth = CurrentDepth;

        if (CurrentDepth <= 6)
        {
            MovePlayerToLayout(CurrentDepth);
        }
        else
        {
            CurrentDepth = 0;
            depth = 0;
            MovePlayerToLayout(0);
        }
    }

    private void MovePlayerToLayout(int targetLayoutIndex)
    {
        if (layoutAnchors == null || layoutAnchors.Length == 0)
        {
            Debug.LogWarning("NextLevelManager: No layout anchors found. Falling back to current position.");
            return;
        }

        Transform targetAnchor = null;
        for (int i = 0; i < layoutAnchors.Length; i++)
        {
            if (layoutAnchors[i] != null && layoutAnchors[i].GetComponent<LevelLayoutAnchor>()?.layoutIndex == targetLayoutIndex)
            {
                targetAnchor = layoutAnchors[i];
                break;
            }
        }

        if (targetAnchor == null)
        {
            Debug.LogWarning($"NextLevelManager: No anchor found for layout {targetLayoutIndex}.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("NextLevelManager: No player found to teleport.");
            return;
        }

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = targetAnchor.position;
            player.transform.rotation = targetAnchor.rotation;
            controller.enabled = true;
        }
        else
        {
            player.transform.position = targetAnchor.position;
            player.transform.rotation = targetAnchor.rotation;
        }

        Debug.Log($"NextLevelManager: Teleported player to layout {targetLayoutIndex}.");
    }

    public void ResetGameEntirely()
    {
        Debug.Log("NextLevelManager: Full game reset triggered.");
    
        // Reset static and instance variables
        CurrentDepth = 0;
        depth = 0;
        enemiesAlive = 0;
        allWavesComplete = false;
        levelEnding = false;
    
        // Clear out any accumulated player upgrades from previous runs
        staticStoredUpgrades.Clear(); 
    
        // Ensure the system time scale is normal
        Time.timeScale = 1f;
        try 
        {
            PauseManager.SetPaused(false);
        }
        catch { }
    }
}

