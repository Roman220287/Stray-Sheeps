using UnityEngine;
using UnityEngine.AI;

public class SlowEffect : MonoBehaviour
{
    private float originalSpeed;
    private float slowMultiplier = 0.5f;
    private float duration;
    private float timeElapsed = 0f;
    private NavMeshAgent agent;

    public void Initialize(float slowDuration, float slowAmount)
    {
        duration = slowDuration;
        slowMultiplier = slowAmount;
        agent = GetComponent<NavMeshAgent>();

        if (agent == null) return;

        originalSpeed = agent.speed;
        agent.speed = originalSpeed * slowMultiplier;
        Debug.Log($"Enemy slowed! Speed: {originalSpeed} -> {agent.speed}");
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed < duration) return;

        if (agent != null)
        {
            agent.speed = originalSpeed;
            Debug.Log($"Slow effect ended. Speed restored to: {originalSpeed}");
        }

        Destroy(this);
    }
}
