using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VendettaAudioManager : MonoBehaviour
{
    public static VendettaAudioManager Instance { get; private set; }

    [Header("Music Tracks")]
    [Tooltip("Main background battle/ambient music (loops)")]
    public AudioClip backgroundMusic;
    [Tooltip("Boss encounter music")]
    public AudioClip bossMusic;
    [Tooltip("Optional game start intro fanfare/stinger played once at beginning")]
    public AudioClip startIntroStinger;

    [Header("Combat & FX SFX")]
    public AudioClip swordSwing;
    public AudioClip swordHit;
    public AudioClip playerHurt;
    public AudioClip enemyDeath;
    public AudioClip lightPickupChime;
    public AudioClip playerDeathStinger;
    public AudioClip victoryFanfare;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ensure AudioSources exist
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0.5f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.9f;

        LoadClipsIfNeeded();
    }

    private void Start()
    {
        // Subscribe to GameEvents
        GameEvents.OnEnemyDefeated += PlayEnemyDeath;
        GameEvents.OnPlayerDied += PlayPlayerDeath;
        GameEvents.OnBossDefeated += PlayVictory;

        // Ensure an AudioListener is active on the main camera
        if (FindFirstObjectByType<AudioListener>() == null)
        {
            Camera cam = Camera.main;
            if (cam != null) cam.gameObject.AddComponent<AudioListener>();
        }

        // Play intro stinger if present, otherwise start normal background music
        if (startIntroStinger != null)
        {
            StartCoroutine(PlayIntroThenBGM());
        }
        else
        {
            PlayNormalMusic();
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnEnemyDefeated -= PlayEnemyDeath;
        GameEvents.OnPlayerDied -= PlayPlayerDeath;
        GameEvents.OnBossDefeated -= PlayVictory;
    }

    private void LoadClipsIfNeeded()
    {
#if UNITY_EDITOR
        if (backgroundMusic == null)
            backgroundMusic = LoadClip("Assets/Audio/song.mp3", "Assets/Vendetta/Audio/song.mp3");

        if (bossMusic == null)
            bossMusic = LoadClip("Assets/Audio/578577__nomiqbomi__dark-crescendo-3.mp3-boss unlocked.mp3", "Assets/Vendetta/Audio/578577__nomiqbomi__dark-crescendo-3.mp3-boss unlocked.mp3");

        if (swordSwing == null)
            swordSwing = LoadClip("Assets/Audio/sword_swing.wav", "Assets/Vendetta/Audio/sword_swing.wav");

        if (swordHit == null)
            swordHit = LoadClip("Assets/Audio/sword_hit.wav", "Assets/Vendetta/Audio/sword_hit.wav");

        if (playerHurt == null)
            playerHurt = swordHit; // default to impact clank

        if (enemyDeath == null)
            enemyDeath = LoadClip("Assets/Audio/enemy_death.wav", "Assets/Vendetta/Audio/enemy_death.wav");

        if (lightPickupChime == null)
            lightPickupChime = LoadClip("Assets/Audio/735168__irolan__item-pickup-chime.wav", "Assets/Audio/270304__littlerobotsoundfactory__collect_point_00.wav");

        if (playerDeathStinger == null)
            playerDeathStinger = LoadClip("Assets/Audio/858212__tommasomotteran__failed-quest-stinger-01-dark-dissonant-orchestra.wav");

        if (victoryFanfare == null)
            victoryFanfare = LoadClip("Assets/Audio/victory.wav", "Assets/Vendetta/Audio/victory.wav");
#endif
    }

#if UNITY_EDITOR
    private static AudioClip LoadClip(params string[] paths)
    {
        foreach (var p in paths)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
            if (clip != null) return clip;
        }
        return null;
    }
#endif

    private IEnumerator PlayIntroThenBGM()
    {
        sfxSource.PlayOneShot(startIntroStinger, 1.0f);
        yield return new WaitForSeconds(Mathf.Min(startIntroStinger.length, 3.5f));
        PlayNormalMusic();
    }

    public void PlayNormalMusic()
    {
        if (backgroundMusic == null) return;
        if (bgmSource.clip == backgroundMusic && bgmSource.isPlaying) return;
        bgmSource.clip = backgroundMusic;
        bgmSource.volume = 0.45f;
        bgmSource.Play();
    }

    public void PlayBossMusic()
    {
        if (bossMusic == null) return;
        bgmSource.Stop();
        bgmSource.clip = bossMusic;
        bgmSource.volume = 0.75f;
        bgmSource.Play();
    }

    public void PlaySwordSwing()
    {
        PlaySoundPitchMod(swordSwing, 0.8f, 0.9f, 1.15f);
    }

    public void PlaySwordHit()
    {
        PlaySoundPitchMod(swordHit, 0.95f, 0.85f, 1.1f);
    }

    public void PlayPlayerHurt()
    {
        PlaySoundPitchMod(playerHurt != null ? playerHurt : swordHit, 1.0f, 0.7f, 0.85f);
    }

    public void PlayEnemyDeath()
    {
        PlaySoundPitchMod(enemyDeath, 0.9f, 0.9f, 1.1f);
    }

    public void PlayPickupChime()
    {
        PlaySoundPitchMod(lightPickupChime, 1.0f, 1.0f, 1.25f);
    }

    public void PlayPlayerDeath()
    {
        bgmSource.Stop();
        if (playerDeathStinger != null) sfxSource.PlayOneShot(playerDeathStinger, 1.0f);
    }

    public void PlayVictory()
    {
        bgmSource.Stop();
        if (victoryFanfare != null)
        {
            sfxSource.PlayOneShot(victoryFanfare, 1.0f);
            Debug.Log("[VendettaAudio] Playing Victory Fanfare!");
        }
    }

    private void PlaySoundPitchMod(AudioClip clip, float vol, float minPitch, float maxPitch)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.PlayOneShot(clip, vol);
        sfxSource.pitch = 1.0f;
    }
}
