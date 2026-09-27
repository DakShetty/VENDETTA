using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died");
        GameEvents.OnPlayerDied?.Invoke();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(100);
        }
    }
}
