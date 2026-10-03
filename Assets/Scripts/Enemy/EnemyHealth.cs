using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    private float currentHealth;

    public GameObject lightDropPrefab;
    public float knockbackForce = 2f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log("Enemy Health: " + currentHealth);

        Vector3 knockbackDirection = transform.position - Camera.main.transform.position;
        knockbackDirection.y = 0;
        knockbackDirection.Normalize();

        transform.position += knockbackDirection * knockbackForce;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy Defeated");

        if (lightDropPrefab != null)
        {
            Instantiate(lightDropPrefab, transform.position + Vector3.up * 0.6f, Quaternion.identity);
        }

        GameEvents.OnEnemyDefeated?.Invoke();

        Destroy(gameObject);
    }
}
