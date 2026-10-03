using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public int requiredLight = 5;
    private int currentLight = 0;

    public GameObject upgradeCanvas;

    public void AddLight(int amount)
    {
        currentLight += amount;
        Debug.Log("Upgrade Progress: " + currentLight + " / " + requiredLight);

        // Strictly trigger upgrade ONLY when all 5 light charges are absorbed (never at 4)
        if (currentLight >= requiredLight)
        {
            StartUpgrade();
        }
    }

    public void StartUpgrade()
    {
        Debug.Log("5 RUNES COLLECTED - RUNE UPGRADE AVAILABLE!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentState = GameManager.GameState.Upgrade;
        }

        if (upgradeCanvas != null)
        {
            upgradeCanvas.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentLight = 0;
    }

    public void ChooseSpeedRune()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();

        if (player != null)
        {
            player.speed += 1.5f;
            Debug.Log("Speed Rune Selected! New Speed: " + player.speed);
        }

        VendettaAudioManager.Instance?.PlayPickupChime();
        FinishUpgrade();
    }

    public void ChooseDamageRune()
    {
        PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();

        if (combat != null)
        {
            combat.attackDamage += 10f;
            Debug.Log("Damage Rune Selected! New Attack Damage: " + combat.attackDamage);
        }

        VendettaAudioManager.Instance?.PlayPickupChime();
        FinishUpgrade();
    }

    public void FinishUpgrade()
    {
        if (upgradeCanvas != null)
        {
            upgradeCanvas.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentState = GameManager.GameState.Playing;
        }
    }
}
