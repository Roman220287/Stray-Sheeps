using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelManager : MonoBehaviour
{
    public static NextLevelManager instance;

    private int enemiesAlive;
    private bool levelEnding;

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

        if (!levelEnding && enemiesAlive <= 0)
        {
            levelEnding = true;
            NextLevel();
        }
    }

    void NextLevel()
    {
        Debug.Log("next level");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
