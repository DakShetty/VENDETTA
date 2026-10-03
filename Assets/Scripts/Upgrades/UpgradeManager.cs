using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public int requiredLight = 5;
    private int currentLight;

    public GameObject upgradeCanvas;

    public void AddLight(int amount)
    {
        currentLight += amount;

        if (currentLight >= requiredLight)
        {
            StartUpgrade();
        }
    }

    void StartUpgrade()
    {
        Debug.Log("UPGRADE AVAILABLE!");

        GameManager.Instance.currentState = GameManager.GameState.Upgrade;

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
            player.speed += 1f;
            Debug.Log("Speed Rune Selected!");
        }

        FinishUpgrade();
    }

    public void ChooseDamageRune()
    {
        PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();

        if (combat != null)
        {
            combat.attackDamage += 10f;
            Debug.Log("Damage Rune Selected!");
        }

        FinishUpgrade();
    }

    void FinishUpgrade()
    {
        if (upgradeCanvas != null)
        {
            upgradeCanvas.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameManager.Instance.currentState = GameManager.GameState.Playing;
    }
}