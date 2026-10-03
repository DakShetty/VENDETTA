using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;

    public GameObject deathCanvas;

    private bool dead;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (dead)
            return;

        currentHealth -= damage;
        VendettaAudioManager.Instance?.PlayPlayerHurt();
        VendettaUIManager.Instance?.TriggerDamageFlash();

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        dead = true;

        Debug.Log("Player Died");

        PlayerMovement movement = GetComponent<PlayerMovement>();

        if (movement != null)
            movement.enabled = false;

        CharacterController controller = GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        if (deathCanvas != null)
            deathCanvas.SetActive(true);

        GameEvents.OnPlayerDied?.Invoke();
    }
}
