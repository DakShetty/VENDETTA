using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Playing,
        Upgrade,
        Boss,
        Victory,
        GameOver
    }

    public GameState currentState;

    public int enemiesDefeated = 0;
    public int killsForUpgrade = 5;
    public int lightCollected = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Time.timeScale = 1f;

        currentState = GameState.Playing;

        GameEvents.OnEnemyDefeated += EnemyDefeated;
        GameEvents.OnPlayerDied += PlayerDied;
        GameEvents.OnBossDefeated += BossDefeated;
    }

    void EnemyDefeated()
    {
        enemiesDefeated++;
        Debug.Log("Enemies Defeated: " + enemiesDefeated);

        // NOTE: Upgrades must strictly trigger ONLY when the player collects all 5 light remains,
        // as per GDD ("Defeat 5 consecutive enemies then collect the remains, upgrade to a rune").
        // UpgradeManager.AddLight() handles StartUpgrade when currentLight >= requiredLight.
    }

    public void CollectLight(int amount)
    {
        lightCollected += amount;
        Debug.Log("Total Light: " + lightCollected);
    }

    public void StartUpgrade()
    {
        currentState = GameState.Upgrade;
        Debug.Log("Upgrade Phase Started");
    }

    void PlayerDied()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f;
        Debug.Log("Game Over");
    }

    void BossDefeated()
    {
        currentState = GameState.Victory;
        Debug.Log("Victory!");
    }

    public bool IsGameOver()
    {
        return currentState == GameState.GameOver;
    }

    void OnDestroy()
    {
        GameEvents.OnEnemyDefeated -= EnemyDefeated;
        GameEvents.OnPlayerDied -= PlayerDied;
        GameEvents.OnBossDefeated -= BossDefeated;
    }
}
