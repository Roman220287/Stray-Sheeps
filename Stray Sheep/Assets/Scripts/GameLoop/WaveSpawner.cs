using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
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
        int randomSpawn = Random.Range(0, spawnPoints.Length);

        Instantiate(
            enemyPrefab,
            spawnPoints[randomSpawn].position,
            Quaternion.identity
        );
    }
}
