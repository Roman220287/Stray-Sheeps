using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Tooltip("The tag assigned to your player GameObject.")]
    [SerializeField] private string playerTag = "Player";

    private Vector3 isometricOffset = new Vector3(0f, 20f, -7f);

    [Tooltip("Additional positional offset applied after the isometric offset.")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    [Tooltip("Fixed camera rotation in Euler angles (degrees). Set this for a top-down/isometric view.")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(69f, 0f, 0f);

    [Tooltip("How long the camera takes to catch up to the player's position. Smaller = snappier.")]
    [SerializeField] private float followSmoothTime = 0.25f;

    [Header("Behavior")]
    [Tooltip("If true, the camera smoothly follows the player's position; rotation stays fixed.")]
    [SerializeField] private bool followPosition = true;

    [Header("Camera Shake")]
    [Tooltip("How quickly the shake moves around.")]
    [SerializeField] private float shakeFrequency = 25f;

    private Transform target;
    private Vector3 currentVelocity = Vector3.zero;

    // Shake variables
    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;

    // Random seeds for Perlin noise
    private float noiseSeedX;
    private float noiseSeedY;

    private void Start()
    {
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(0f, 1000f);

        GameObject player = GameObject.FindWithTag(playerTag);

        if (player != null)
        {
            target = player.transform;

            transform.position = target.position + isometricOffset + positionOffset;
            transform.rotation = Quaternion.Euler(fixedRotation);
        }
        else
        {
            Debug.LogError($"SmoothCameraFollow: Could not find a GameObject with the tag '{playerTag}'.");
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 shakeOffset = Vector3.zero;

        if (shakeTimer > 0f)
        {
            float strength = shakeTimer / shakeDuration;

            float x = (Mathf.PerlinNoise(
                noiseSeedX,
                Time.time * shakeFrequency) - 0.5f) * 2f;

            float y = (Mathf.PerlinNoise(
                noiseSeedY,
                Time.time * shakeFrequency) - 0.5f) * 2f;

            shakeOffset = new Vector3(x, y, 0f) *
            shakeMagnitude * strength;
            shakeTimer -= Time.deltaTime;
        }

        if (followPosition)
        {
            Vector3 desiredPos =
                target.position +
                isometricOffset +
                positionOffset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPos,
                ref currentVelocity,
                followSmoothTime);

            // Apply shake AFTER smoothing
            transform.position += shakeOffset;
        }

        transform.rotation = Quaternion.Euler(fixedRotation);
    }

    /// <summary>
    /// Triggers a camera shake.
    /// If another shake is already active, the stronger/longer values are kept.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeTimer = Mathf.Max(shakeTimer, duration);
        shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
    }
}