using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class SpawnDropEffect : MonoBehaviour
{
    [Header("Spawn Drop Settings")]
    public float spawnHeight = 8f;
    public float slowDescendDuration = 1.5f;
    public float releaseHeight = 2f;
    public float extraFallForce = 10f;
    public float landingCheckDistance = 0.2f;
    public float landingSettleTime = 0.2f;

    [Header("String Visual")]
    public bool showString = true;
    public Material stringMaterial;
    public float stringWidth = 0.05f;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private EnemyBase enemyAI;
    private LineRenderer lineRenderer;

    private Vector3 groundPosition;
    private float startY;
    private float releaseY;
    private float descendStartTime;
    private bool isDescending;
    private bool isDropping;
    private bool hasLanded;
    private float landTime;
    private bool initialized;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        enemyAI = GetComponent<EnemyBase>();
        if (enemyAI == null)
            enemyAI = GetComponentInChildren<EnemyBase>();

        if (agent != null)
            agent.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (enemyAI != null)
            enemyAI.enabled = false;

        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        if (!showString) return;

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = stringWidth;
        lineRenderer.material = stringMaterial != null
            ? stringMaterial
            : new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    public void Initialize(Vector3 targetGround)
    {
        groundPosition = targetGround;
        Vector3 spawnPosition = targetGround + Vector3.up * spawnHeight;
        transform.position = spawnPosition;

        startY = spawnPosition.y;
        releaseY = targetGround.y + releaseHeight;
        descendStartTime = Time.time;
        isDescending = true;
        isDropping = false;
        hasLanded = false;
        landTime = 0f;
        initialized = true;
    }

    private void Update()
    {
        if (PauseManager.IsPaused) return;
        if (!initialized) return;

        if (isDescending)
        {
            float elapsed = Time.time - descendStartTime;
            float t = Mathf.Clamp01(elapsed / slowDescendDuration);
            float currentY = Mathf.Lerp(startY, releaseY, t);
            transform.position = new Vector3(transform.position.x, currentY, transform.position.z);

            if (t >= 1f)
            {
                StartDrop();
            }
        }
        else if (isDropping)
        {
            if (rb == null) return;

            if (rb.linearVelocity.y < 0f)
                rb.AddForce(Vector3.down * extraFallForce, ForceMode.Acceleration);

            Vector3 rayOrigin = transform.position + Vector3.up * 0.25f;
            float rayDistance = landingCheckDistance + 1f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance))
            {
                if (hit.normal.y > 0.7f)
                {
                    landTime += Time.deltaTime;
                    if (landTime >= landingSettleTime)
                    {
                        LandOnGround(hit.point);
                    }
                }
                else
                {
                    landTime = 0f;
                }
            }
            else
            {
                landTime = 0f;
            }
        }

        UpdateStringRenderer();
    }

    private void StartDrop()
    {
        isDescending = false;
        isDropping = true;
        landTime = 0f;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }

        if (agent != null)
            agent.enabled = false;

        if (lineRenderer != null)
            lineRenderer.positionCount = 2;
    }

    private void LandOnGround(Vector3 contactPoint)
    {
        if (!isDropping || hasLanded) return;

        hasLanded = true;
        isDropping = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }

        transform.position = new Vector3(transform.position.x, contactPoint.y, transform.position.z);

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
            agent.ResetPath();
            agent.isStopped = false;
        }

        if (enemyAI != null)
            enemyAI.enabled = true;

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    private void UpdateStringRenderer()
    {
        if (lineRenderer == null || !showString) return;

        if (hasLanded)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        Vector3 topPoint = groundPosition + Vector3.up * spawnHeight;
        lineRenderer.SetPosition(0, topPoint);
        lineRenderer.SetPosition(1, transform.position);
    }
}
