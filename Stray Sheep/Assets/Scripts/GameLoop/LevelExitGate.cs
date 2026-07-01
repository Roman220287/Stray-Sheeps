using UnityEngine;

public class LevelExitGate : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;
    
    private bool isGateOpen = false;

    private void Awake()
    {
        // Start with the gate closed
        if (closedVisual != null) closedVisual.SetActive(true);
        if (openVisual != null) openVisual.SetActive(false);
    }

    public void OpenGate()
    {
        isGateOpen = true;
        
        if (closedVisual != null) closedVisual.SetActive(false);
        if (openVisual != null) openVisual.SetActive(true);
        
        Debug.Log("The exit gate is now open! Proceed when ready.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGateOpen && other.CompareTag("Player"))
        {
            isGateOpen = false;
            Debug.Log("Player entered the gate. Loading next level...");
            
            if (NextLevelManager.instance != null)
            {
                NextLevelManager.instance.ProceedToNextLevel();
            }
        }
    }
}
