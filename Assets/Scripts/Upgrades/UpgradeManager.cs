using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public int requiredLight = 5;
    public int currentLight = 0;

    public GameObject upgradeCanvas;

    public void AddLight(int amount)
    {
        currentLight += amount;
        Debug.Log("[UpgradeManager] Souls Harvested: " + currentLight + " / " + requiredLight);

        // Strictly trigger ONLY when 5 souls are harvested (never at 4)
        if (currentLight >= requiredLight)
        {
            StartUpgrade();
        }
    }

    public void StartUpgrade()
    {
        Debug.Log("ALL 5 SOULS HARVESTED - ASCENSION READY!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentState = GameManager.GameState.Upgrade;
        }

        // Open our Ascension Modal
        if (VendettaUIManager.Instance != null)
        {
            VendettaUIManager.Instance.OpenAscensionModal();
        }
        else if (upgradeCanvas != null)
        {
            upgradeCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        currentLight = 0;
    }

    public void ChooseSpeedRune()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();

        if (player != null)
        {
            player.speed += 1.75f;
            Debug.Log("Rune of Swiftness Embodied! Speed: " + player.speed);
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
            Debug.Log("Rune of Fury Embodied! Attack Damage: " + combat.attackDamage);
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
