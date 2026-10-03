using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform bossSpawnPoint;
    public GameObject boss;

    public int totalEnemies = 5;

    private int enemiesSpawned = 0;

    void Start()
    {
        GameEvents.OnEnemyDefeated += SpawnNextEnemy;

        SpawnNextEnemy();
    }

    void SpawnNextEnemy()
    {
        if (enemiesSpawned >= totalEnemies)
        {
            SpawnBoss();
            return;
        }

        if (enemyPrefab != null && spawnPoint != null)
        {
            Instantiate(
                enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            enemiesSpawned++;

            Debug.Log("Enemy Spawned: " + enemiesSpawned);
        }
    }

    void SpawnBoss()
    {
        if (boss != null)
        {
            boss.transform.position = bossSpawnPoint.position;
            boss.transform.rotation = bossSpawnPoint.rotation;

            boss.SetActive(true);
            VendettaAudioManager.Instance?.PlayBossMusic();
            VendettaUIManager.Instance?.ShowBossBar(true);

            Debug.Log("BOSS SPAWNED!");
        }
    }

    void OnDestroy()
    {
        GameEvents.OnEnemyDefeated -= SpawnNextEnemy;
    }
}

