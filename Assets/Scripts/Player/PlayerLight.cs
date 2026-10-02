using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    public int lightAmount = 0;

    public void AbsorbLight(int amount)
    {
        lightAmount += amount;

        Debug.Log("Player Light: " + lightAmount);

        GameManager.Instance.CollectLight(amount);

        UpgradeManager upgradeManager = FindFirstObjectByType<UpgradeManager>();

        if (upgradeManager != null)
        {
            upgradeManager.AddLight(amount);
        }
    }
}