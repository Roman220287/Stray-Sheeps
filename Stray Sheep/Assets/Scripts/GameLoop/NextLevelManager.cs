using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NextLevelManager : MonoBehaviour
{
    public static NextLevelManager instance;

    private int enemiesAlive;
    private bool allWavesComplete = false;
    private bool levelEnding = false;

    private void Awake()
    {
        instance = this;
    }

    public void RegisterEnemy()
    {
        enemiesAlive += 1;
    }

    public void UnregisterEnemy()
    {
        enemiesAlive -= 1;

        if (!levelEnding && allWavesComplete && enemiesAlive <= 0)
        {
            levelEnding = true;
            LoadNextLevel();
        }
    }

    public void WavesFinished()
    {
        allWavesComplete = true;

        UnregisterEnemy();
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(0.5f);

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        if (currentIndex + 1 < totalScenes)
        {
            SceneManager.LoadScene(currentIndex + 1);
        }
        else
        {
            Debug.Log("Prototype Complete!");
        }
    }
}
