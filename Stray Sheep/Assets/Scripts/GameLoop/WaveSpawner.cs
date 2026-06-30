using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("Base Enemy Pools")]
    public GameObject[] enemyPrefabs;

    [Header("Depth-specific Enemy Pools")]
    [SerializeField] private GameObject[] depth1Enemies;
    [SerializeField] private GameObject[] depth2Enemies;
    [SerializeField] private GameObject[] depth3Enemies;
    [SerializeField] private GameObject[] depth4Enemies;
    [SerializeField] private GameObject[] depth5Enemies;
    [SerializeField] private GameObject[] depth6Enemies;

    [Header("Spawner Settings")]
    [SerializeField] private int extraSpawnPointsAtDepth3 = 1;
    [SerializeField] private int extraSpawnPointsAtDepth6 = 2;
    public Transform[] spawnPoints;
    public int numberOfWaves;
    public int enemiesPerWave;
    public float timeBetweenSpawns;
    public float timeBetweenWaves;

    // 1. Move everything to OnEnable. This runs EVERY time a scene loads/reloads, 
    // even if Awake had a minor hiccup.
    void OnEnable()
    {
        try 
        {
            PauseManager.SetPaused(false);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"WaveSpawner safely bypassed a PauseManager error on reload: {e.Message}");
        }

        Time.timeScale = 1f;

        // Force a fresh start of the spawning logic
        StopAllCoroutines();
        StartCoroutine(SpawnWaves());
    }

    // Remove or empty your old Start() method so they don't fight
    void Start()
    {
        // Kept empty to prevent duplicate triggers
    }

    IEnumerator SpawnWaves()
    {
        for (int wave = 0; wave < numberOfWaves; wave++)
        {
            Debug.Log($"WaveSpawner: Starting wave {wave + 1}/{numberOfWaves} with {enemiesPerWave} enemies.");
            for (int i = 0; i < enemiesPerWave; i++)
            {
                // Only stall if it isn't the absolute first spawn of the entire level
                if (wave > 0 || i > 0)
                {
                    while (PauseManager.IsPaused)
                        yield return null;
                }

                SpawnEnemy();

                yield return new WaitForSecondsRealtime(timeBetweenSpawns);
            }

            while (PauseManager.IsPaused)
                yield return null;

            yield return new WaitForSecondsRealtime(timeBetweenWaves);
        }

        NextLevelManager.instance.WavesFinished();
    }

    void SpawnEnemy()
    {
        // kies een willekeurige vijand uit de beschikbare pool voor de huidige diepte en spawn deze op een willekeurige spawnpositie
        GameObject[] availableEnemies = GetEnemyPoolForCurrentDepth();
        if (availableEnemies == null || availableEnemies.Length == 0)
        {
            Debug.LogWarning("WaveSpawner has no enemy prefabs assigned for the current depth.");
            return;
        }

        Transform[] activeSpawnPoints = GetActiveSpawnPoints();
        if (activeSpawnPoints == null || activeSpawnPoints.Length == 0)
        {
            Debug.LogWarning("WaveSpawner has no spawn points assigned or found under this spawner.");
            return;
        }

        int randomSpawn = Random.Range(0, activeSpawnPoints.Length);
        int randomEnemy = Random.Range(0, availableEnemies.Length);
        Vector3 spawnPosition = activeSpawnPoints[randomSpawn].position;

        GameObject enemyInstance = Instantiate(
            availableEnemies[randomEnemy],
            spawnPosition,
            Quaternion.identity
        );

        SpawnDropEffect dropEffect = enemyInstance.GetComponent<SpawnDropEffect>();
        if (dropEffect != null)
        {
            dropEffect.Initialize(spawnPosition);
        }
    }

    private GameObject[] GetEnemyPoolForCurrentDepth()
    {
        // bepaal de huidige diepte en kies de juiste vijandpool
        int currentDepth = NextLevelManager.instance != null ? NextLevelManager.instance.depth : 0;

        if (currentDepth >= 6 && depth6Enemies != null && depth6Enemies.Length > 0)
            return depth6Enemies;
        if (currentDepth >= 5 && depth5Enemies != null && depth5Enemies.Length > 0)
            return depth5Enemies;
        if (currentDepth >= 4 && depth4Enemies != null && depth4Enemies.Length > 0)
            return depth4Enemies;
        if (currentDepth >= 3 && depth3Enemies != null && depth3Enemies.Length > 0)
            return depth3Enemies;
        if (currentDepth >= 2 && depth2Enemies != null && depth2Enemies.Length > 0)
            return depth2Enemies;
        if (currentDepth >= 1 && depth1Enemies != null && depth1Enemies.Length > 0)
            return depth1Enemies;

        return enemyPrefabs;
    }

    private Transform[] GetActiveSpawnPoints()
    {
        // verzamel alle unieke spawnpunten van de opgegeven array en de kinderen van deze spawner, en pas het aantal aan op basis van de huidige diepte
        List<Transform> candidates = new List<Transform>();

        if (spawnPoints != null)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null && !candidates.Contains(spawnPoints[i]))
                    candidates.Add(spawnPoints[i]);
            }
        }

        Transform[] childSpawners = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childSpawners.Length; i++)
        {
            if (childSpawners[i] != transform && !candidates.Contains(childSpawners[i]))
                candidates.Add(childSpawners[i]);
        }

        int currentDepth = NextLevelManager.instance != null ? NextLevelManager.instance.depth : 0;
        int baseSpawnCount = spawnPoints != null ? spawnPoints.Length : 0;
        int targetCount = baseSpawnCount;

        if (currentDepth >= 6)
            targetCount += extraSpawnPointsAtDepth6;
        else if (currentDepth >= 3)
            targetCount += extraSpawnPointsAtDepth3;

        targetCount = Mathf.Clamp(targetCount, 1, candidates.Count);
        return candidates.GetRange(0, targetCount).ToArray();
    }
}
