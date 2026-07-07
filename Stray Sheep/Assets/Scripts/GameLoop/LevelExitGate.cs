using UnityEngine;

public class LevelExitGate : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;
    [SerializeField] private int gateLayoutIndex = -1;
    
    private bool isGateOpen = false;

    private void Awake()
    {
        isGateOpen = false;
        // Start with the gate closed
        if (closedVisual != null) closedVisual.SetActive(true);
        if (openVisual != null) openVisual.SetActive(false);
        
        // Ensure this GameObject has a trigger collider
        Collider gateCollider = GetComponent<Collider>();
        if (gateCollider != null)
        {
            Debug.Log($"LevelExitGate: Gate {gateLayoutIndex} collider found. Is Trigger: {gateCollider.isTrigger}");
        }
        else
        {
            Debug.LogError($"LevelExitGate: Gate {gateLayoutIndex} is MISSING a Collider! Add a collider and set 'Is Trigger' to true.");
        }
    }

    public int GetGateLayoutIndex() => gateLayoutIndex;

    public void OpenGate()
    {
        isGateOpen = true;
        
        if (closedVisual != null) closedVisual.SetActive(false);
        if (openVisual != null) openVisual.SetActive(true);
        
        Debug.Log($"LevelExitGate: Gate {gateLayoutIndex} is now open! Proceed when ready.");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"GATE TRIGGER! Other: {other.gameObject.name}, Tag: {other.tag}, Gate Open: {isGateOpen}");
        
        if (isGateOpen && other.CompareTag("Player"))
        {
            isGateOpen = false;
            Debug.Log("Player entered the gate. Loading next level...");
            
            NextLevelManager manager = NextLevelManager.ResolveInstance();
            if (manager != null)
            {
                Debug.Log("LevelExitGate: Manager found. Calling ProceedToNextLevel().");
                manager.ProceedToNextLevel();
            }
            else
            {
                Debug.LogWarning("LevelExitGate: Manager could not be resolved.");
            }
        }
        else
        {
            if (!isGateOpen)
                Debug.Log($"Gate {gateLayoutIndex} is NOT open yet.");
            if (!other.CompareTag("Player"))
                Debug.Log($"Collider '{other.gameObject.name}' is not tagged as 'Player'.");
        }
    }
}
