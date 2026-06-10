using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NextLevelManager : MonoBehaviour
{
    public static NextLevelManager instance;
    public static int CurrentDepth { get; private set; }

    public ChooseUpgradeMenu upgradeMenu;

    private int enemiesAlive;
    private bool allWavesComplete = false;
    private bool levelEnding = false;
    [SerializeField] public int depth = 0;

    private void Awake()
    {
        instance = this;
        depth = CurrentDepth;
    }

    public void RegisterEnemy()
    {
        enemiesAlive += 1;
    }

    public void UnregisterEnemy()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        CheckLevelComplete();
    }

    public void WavesFinished()
    {
        allWavesComplete = true;
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        if (!levelEnding && allWavesComplete && enemiesAlive <= 0)
        {
            levelEnding = true;
            if (upgradeMenu != null)
            {
                upgradeMenu.ShowMenu();
            }
            else
            {
                StartCoroutine(LoadNextLevel());
            }
        }
    }

    public void ProceedToNextLevel()
    {
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        CurrentDepth = depth + 1;
        depth = CurrentDepth;

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
