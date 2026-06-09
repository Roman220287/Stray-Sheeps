using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private float shakeDuration = 0f;

    [Header("Settings")]
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    void Awake()
    {
        // Store the camera's original local position
        originalPosition = transform.localPosition;
    }

    void shake()
    {
        if (shakeDuration > 0)
        {
            // Apply a random offset within the shakeAmount range
            transform.localPosition = originalPosition + Random.insideUnitSphere * shakeAmount;

            // Gradually reduce the duration
            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            // Reset to original position when finished
            shakeDuration = 0f;
            transform.localPosition = originalPosition;
        }
    }

    /// <summary>
    /// Call this method to trigger the shake effect.
    /// </summary>
    /// <param name="duration">How long the shake lasts.</param>
    /// <param name="amount">The intensity of the shake.</param>
    public void TriggerShake(float duration, float amount)
    {
        shakeDuration = duration;
        shakeAmount = amount;
        shake();
    }
}
