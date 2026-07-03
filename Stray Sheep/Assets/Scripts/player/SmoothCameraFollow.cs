using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Stage Settings")]
    [Tooltip("The center point of your stage/level.")]
    [SerializeField] private Vector3 stageCenter = Vector3.zero;
    [Tooltip("How much the player's movement shifts the camera away from center.")]
    [Range(0f, 0.5f)] [SerializeField] private float driftStrength = 0.2f;

    [Header("Camera Setup")]
    [SerializeField] private string playerTag = "Player";
    private Vector3 isometricOffset = new Vector3(0f, 20f, -7f);
    [SerializeField] private Vector3 fixedRotation = new Vector3(69f, 0f, 0f);
    [SerializeField] private float followSmoothTime = 0.3f;

    private Transform target;
    private Vector3 currentVelocity = Vector3.zero;

    // Shake variables
    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;
    private float noiseSeedX;
    private float noiseSeedY;

    private void Start()
    {
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(0f, 1000f);
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null) target = player.transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // The base position is the CENTER of the stage, not the player.
        // We add a small fraction of the player's position relative to center to create "drift."
        Vector3 playerDrift = (target.position - stageCenter) * driftStrength;
        Vector3 desiredPos = stageCenter + isometricOffset + playerDrift;

        // Shake logic
        Vector3 shakeOffset = Vector3.zero;
        if (shakeTimer > 0f)
        {
            float strength = shakeTimer / shakeDuration;
            float x = (Mathf.PerlinNoise(noiseSeedX, Time.time * 25f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(noiseSeedY, Time.time * 25f) - 0.5f) * 2f;
            shakeOffset = new Vector3(x, y, 0f) * shakeMagnitude * strength;
            shakeTimer -= Time.deltaTime;
        }

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, followSmoothTime) + shakeOffset;
        transform.rotation = Quaternion.Euler(fixedRotation);
    }

    public void SetStageCenter(Vector3 newCenter)
    {
        stageCenter = newCenter;
        Debug.Log($"SmoothCameraFollow: Updated stage center to {stageCenter}");
    }

    public void Shake(float duration, float magnitude)
    {
        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeTimer = Mathf.Max(shakeTimer, duration);
        shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
    }
}