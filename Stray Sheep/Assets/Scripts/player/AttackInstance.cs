using UnityEngine;

public class AttackInstance : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 1.5f;
    public float speed = 15f;
    public bool isProjectile = true;

    private void Start()
    {
        // Use transform.root to ensure we destroy the top-most parent after the lifetime ends
        Destroy(transform.root.gameObject, lifetime);
    }

    private void Update()
    {
        if (isProjectile)
        {
            // Move the root object so the children follow correctly
            transform.root.Translate(transform.root.forward * speed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit Enemy with child collider");
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            
            // Destroy the top-most parent object on hit
            Destroy(transform.root.gameObject);
        }
        else if (other.CompareTag("Environment"))
        {
            Destroy(transform.root.gameObject);
        }
    }
}
