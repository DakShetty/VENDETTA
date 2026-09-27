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
        currentState = GameState.Playing;

        GameEvents.OnEnemyDefeated += EnemyDefeated;
        GameEvents.OnPlayerDied += PlayerDied;
        GameEvents.OnBossDefeated += BossDefeated;
    }

    void EnemyDefeated()
    {
        enemiesDefeated++;

        Debug.Log("Enemies Defeated: " + enemiesDefeated);

        if (enemiesDefeated >= killsForUpgrade)
        {
            StartUpgrade();
        }
    }

    void StartUpgrade()
    {
        currentState = GameState.Upgrade;
        Debug.Log("Upgrade Phase Started");
    }

    void PlayerDied()
    {
        currentState = GameState.GameOver;
        Debug.Log("Game Over");
    }

    void BossDefeated()
    {
        currentState = GameState.Victory;
        Debug.Log("Victory!");
    }

    void OnDestroy()
    {
        GameEvents.OnEnemyDefeated -= EnemyDefeated;
        GameEvents.OnPlayerDied -= PlayerDied;
        GameEvents.OnBossDefeated -= BossDefeated;
    }
}