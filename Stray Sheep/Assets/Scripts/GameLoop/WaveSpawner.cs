using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public int numberOfWaves;
    public int enemiesPerWave;
    public float timeBetweenSpawns;
    public float timeBetweenWaves;


    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        for (int wave = 0; wave < numberOfWaves; wave++)
        {
            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnEnemy();

                yield return new WaitForSeconds(timeBetweenSpawns);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        NextLevelManager.instance.WavesFinished();
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("WaveSpawner has no enemy prefabs assigned.");
            return;
        }

        int randomSpawn = Random.Range(0, spawnPoints.Length);
        int randomEnemy = Random.Range(0, enemyPrefabs.Length);
        Vector3 spawnPosition = spawnPoints[randomSpawn].position;

        GameObject enemyInstance = Instantiate(
            enemyPrefabs[randomEnemy],
            spawnPosition,
            Quaternion.identity
        );

        SpawnDropEffect dropEffect = enemyInstance.GetComponent<SpawnDropEffect>();
        if (dropEffect != null)
        {
            dropEffect.Initialize(spawnPosition);
        }
    }
}
