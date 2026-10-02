using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 500f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log("Boss Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss Defeated!");

        GameEvents.OnBossDefeated?.Invoke();

        Destroy(gameObject);
    }
}
