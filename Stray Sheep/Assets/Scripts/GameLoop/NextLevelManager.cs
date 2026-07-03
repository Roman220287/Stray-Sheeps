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
    [SerializeField] private float cameraSettleTime = 1f;

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

    public void OpenCurrentLayoutGate()
    {
        Debug.Log($"NextLevelManager: OpenCurrentLayoutGate called. Current depth: {depth}");
        OpenGateForLayout(depth);
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(layoutTransitionDelay);

        CurrentDepth = depth + 1;
        depth = CurrentDepth;

        // Start the transition
        TransitionScreen transition = FindFirstObjectByType<TransitionScreen>();
        if (transition != null)
        {
            transition.ShowImmediately();
        }

        if (CurrentDepth <= 6)
        {
            yield return StartCoroutine(TransitionToLayout(CurrentDepth, transition));
        }
        else
        {
            CurrentDepth = 0;
            depth = 0;
            yield return StartCoroutine(TransitionToLayout(0, transition));
        }
    }

    private IEnumerator TransitionToLayout(int targetLayoutIndex, TransitionScreen transition)
    {
        // Move player and update camera during fade
        MovePlayerToLayout(targetLayoutIndex);

        // Wait for camera to settle
        yield return new WaitForSecondsRealtime(cameraSettleTime);

        // Fade out transition
        if (transition != null)
        {
            yield return StartCoroutine(FadeOutTransition(transition));
        }
    }

    private IEnumerator FadeOutTransition(TransitionScreen transition)
    {
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = transition.GetComponent<CanvasGroup>();

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private void OpenGateForLayout(int targetLayoutIndex)
    {
        LevelExitGate[] gates = FindObjectsByType<LevelExitGate>(FindObjectsSortMode.None);
        Debug.Log($"NextLevelManager: Looking for gate with layout index {targetLayoutIndex}. Found {gates.Length} gates.");
        
        foreach (var gate in gates)
        {
            if (gate != null)
            {
                Debug.Log($"  Checking gate: gateLayoutIndex={gate.GetGateLayoutIndex()}");
                if (gate.GetGateLayoutIndex() == targetLayoutIndex)
                {
                    gate.OpenGate();
                    Debug.Log($"NextLevelManager: ✓ Opened gate for layout {targetLayoutIndex}.");
                    return;
                }
            }
        }

        Debug.LogWarning($"NextLevelManager: ✗ No gate found for layout {targetLayoutIndex}. Available gates: {string.Join(", ", System.Linq.Enumerable.Select(gates, g => g?.GetGateLayoutIndex().ToString() ?? "null"))}");
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

        // Update camera to center on the middle of this layout
        SmoothCameraFollow cameraFollow = FindFirstObjectByType<SmoothCameraFollow>();
        LevelLayoutAnchor layoutAnchor = targetAnchor.GetComponent<LevelLayoutAnchor>();
        if (cameraFollow != null)
        {
            Vector3 layoutCenterPos = (layoutAnchor != null) ? layoutAnchor.GetLayoutCenterPosition() : targetAnchor.position;
            cameraFollow.SetStageCenter(layoutCenterPos);
        }

        // Reset the wave spawner for the new layout
        WaveSpawner waveSpawner = FindFirstObjectByType<WaveSpawner>();
        if (waveSpawner != null)
        {
            // Reset level ending flag so the next layout can trigger win condition properly
            levelEnding = false;
            allWavesComplete = false;
            enemiesAlive = 0;
            
            // Delay wave restart until after transition is complete (1.5 seconds: cameraSettleTime + fade duration)
            waveSpawner.RestartWavesWithDelay(cameraSettleTime + 0.5f);
            Debug.Log($"NextLevelManager: Scheduled wave spawner restart for layout {targetLayoutIndex}.");
        }

        // Open the gate for this layout
        OpenGateForLayout(targetLayoutIndex);

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

