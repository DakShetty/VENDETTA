using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VendettaUIManager : MonoBehaviour
{
    public static VendettaUIManager Instance { get; private set; }

    [Header("In-Game HUD")]
    public Canvas hudCanvas;
    public Image vitalityFill;
    public Text vitalityText;
    public Image damageFlashOverlay;
    public Image bossHealthFill;
    public GameObject bossBarContainer;
    public Text soulCounterText;

    [Header("Rune Ascension Modal (High Quality UI)")]
    public GameObject runeModalPanel;
    public Sprite cardFrameSprite;
    public Sprite speedRuneSprite;
    public Sprite damageRuneSprite;

    [Header("Victory Screen")]
    public GameObject victoryBannerPanel;

    private PlayerHealth playerHealth;
    private PlayerLight playerLight;
    private BossHealth bossHealth;
    private UpgradeManager upgradeManager;

    private bool showDebugSoundboard = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadSpritesIfNeeded();
    }

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerLight = FindFirstObjectByType<PlayerLight>();
        upgradeManager = FindFirstObjectByType<UpgradeManager>();
        bossHealth = FindFirstObjectByType<BossHealth>();

        GameEvents.OnBossDefeated += ShowVictoryScreen;

        BuildHUDAndRuneModalIfMissing();

        if (bossBarContainer != null) bossBarContainer.SetActive(false);
        if (runeModalPanel != null) runeModalPanel.SetActive(false);
        if (victoryBannerPanel != null) victoryBannerPanel.SetActive(false);

        // Hide teammate's old placeholder upgradeCanvas so only our HD modal displays
        if (upgradeManager != null && upgradeManager.upgradeCanvas != null)
        {
            upgradeManager.upgradeCanvas.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnBossDefeated -= ShowVictoryScreen;
    }

    private void Update()
    {
        // Keep references alive
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerLight == null) playerLight = FindFirstObjectByType<PlayerLight>();
        if (upgradeManager == null) upgradeManager = FindFirstObjectByType<UpgradeManager>();
        if (bossHealth == null) bossHealth = FindFirstObjectByType<BossHealth>();

        // Update player health bar (both fillAmount and anchorMax for 100% reliability)
        if (playerHealth != null && vitalityFill != null && playerHealth.maxHealth > 0)
        {
            float hpPct = Mathf.Clamp01(playerHealth.currentHealth / playerHealth.maxHealth);
            vitalityFill.rectTransform.anchorMax = new Vector2(hpPct, 1f);
            vitalityFill.fillAmount = hpPct;

            if (vitalityText != null)
            {
                vitalityText.text = "HP: " + Mathf.CeilToInt(playerHealth.currentHealth) + " / " + Mathf.CeilToInt(playerHealth.maxHealth);
            }
        }

        // Update soul counter text
        if (soulCounterText != null)
        {
            int souls = playerLight != null ? playerLight.lightAmount : 0;
            int req = upgradeManager != null ? upgradeManager.requiredLight : 5;
            soulCounterText.text = "LIGHT SOULS: " + souls + " / " + req;
        }

        // Check if Upgrade phase was triggered
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Upgrade)
        {
            if (runeModalPanel != null && !runeModalPanel.activeSelf)
            {
                OpenRuneModal();
            }
        }

        // Update boss health bar if boss is active
        if (bossBarContainer != null && bossBarContainer.activeSelf && bossHealth != null && bossHealth.maxHealth > 0)
        {
            float bossPct = Mathf.Clamp01(bossHealth.currentHealth / bossHealth.maxHealth);
            if (bossHealthFill != null)
            {
                bossHealthFill.rectTransform.anchorMax = new Vector2(bossPct, 1f);
                bossHealthFill.fillAmount = bossPct;
            }
        }

        // Hotkeys for testing
        if (Input.GetKeyDown(KeyCode.R)) ToggleRuneModal();
        if (Input.GetKeyDown(KeyCode.K)) { if (playerLight != null) playerLight.AbsorbLight(1); }
        if (Input.GetKeyDown(KeyCode.B)) ToggleBossBar();
        if (Input.GetKeyDown(KeyCode.F1)) showDebugSoundboard = !showDebugSoundboard;
    }

    public void TriggerDamageFlash()
    {
        if (damageFlashOverlay != null)
        {
            StopCoroutine("DamageFlashRoutine");
            StartCoroutine("DamageFlashRoutine");
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        damageFlashOverlay.color = new Color(0.85f, 0.05f, 0.05f, 0.35f);
        float elapsed = 0f;
        float duration = 0.25f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(0.35f, 0f, elapsed / duration);
            damageFlashOverlay.color = new Color(0.85f, 0.05f, 0.05f, a);
            yield return null;
        }
        damageFlashOverlay.color = new Color(0.85f, 0.05f, 0.05f, 0f);
    }

    public void ShowVictoryScreen()
    {
        if (victoryBannerPanel != null)
        {
            victoryBannerPanel.SetActive(true);
        }
        ShowBossBar(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LoadSpritesIfNeeded()
    {
#if UNITY_EDITOR
        if (cardFrameSprite == null)
            cardFrameSprite = LoadSprite("Assets/UI/T_Card_Frame_HD.png", "Assets/Vendetta/UI/T_Card_Frame_HD.png");
        if (speedRuneSprite == null)
            speedRuneSprite = LoadSprite("Assets/UI/T_Rune_Speed_HD.png", "Assets/Vendetta/UI/T_Rune_Speed_HD.png");
        if (damageRuneSprite == null)
            damageRuneSprite = LoadSprite("Assets/UI/T_Rune_Damage_HD.png", "Assets/Vendetta/UI/T_Rune_Damage_HD.png");
#endif
    }

#if UNITY_EDITOR
    private static Sprite LoadSprite(params string[] paths)
    {
        foreach (var p in paths)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (s != null) return s;
        }
        return null;
    }
#endif

    public void OpenRuneModal()
    {
        if (runeModalPanel != null)
        {
            runeModalPanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void CloseRuneModal()
    {
        if (runeModalPanel != null)
        {
            runeModalPanel.SetActive(false);
            Time.timeScale = 1f;
        }
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Upgrade)
        {
            GameManager.Instance.currentState = GameManager.GameState.Playing;
        }
    }

    public void ToggleRuneModal()
    {
        if (runeModalPanel != null)
        {
            if (runeModalPanel.activeSelf) CloseRuneModal();
            else OpenRuneModal();
        }
    }

    public void SelectSpeedRune()
    {
        if (upgradeManager != null) upgradeManager.ChooseSpeedRune();
        else
        {
            var move = FindFirstObjectByType<PlayerMovement>();
            if (move != null) move.speed += 1.5f;
        }

        VendettaAudioManager.Instance?.PlayPickupChime();
        CloseRuneModal();
    }

    public void SelectDamageRune()
    {
        if (upgradeManager != null) upgradeManager.ChooseDamageRune();
        else
        {
            var combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.attackDamage += 15f;
        }

        VendettaAudioManager.Instance?.PlaySwordHit();
        CloseRuneModal();
    }

    public void ShowBossBar(bool show)
    {
        if (bossBarContainer != null) bossBarContainer.SetActive(show);
    }

    public void ToggleBossBar()
    {
        if (bossBarContainer != null)
        {
            bool next = !bossBarContainer.activeSelf;
            bossBarContainer.SetActive(next);
            if (next) VendettaAudioManager.Instance?.PlayBossMusic();
            else VendettaAudioManager.Instance?.PlayNormalMusic();
        }
    }

    private void BuildHUDAndRuneModalIfMissing()
    {
        if (hudCanvas == null)
        {
            var cGO = new GameObject("Vendetta_HD_Canvas");
            hudCanvas = cGO.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cGO.AddComponent<GraphicRaycaster>();
        }

        // Damage flash screen overlay
        if (damageFlashOverlay == null)
        {
            var flashGO = new GameObject("DamageFlashOverlay", typeof(RectTransform), typeof(Image));
            flashGO.transform.SetParent(hudCanvas.transform, false);
            var rtFlash = flashGO.GetComponent<RectTransform>();
            rtFlash.anchorMin = Vector2.zero;
            rtFlash.anchorMax = Vector2.one;
            rtFlash.sizeDelta = Vector2.zero;
            damageFlashOverlay = flashGO.GetComponent<Image>();
            damageFlashOverlay.color = new Color(1, 0, 0, 0);
            damageFlashOverlay.raycastTarget = false;
        }

        // 1. Vitality Bar (Health)
        if (vitalityFill == null)
        {
            var barBgGO = new GameObject("HealthBar_BG", typeof(RectTransform), typeof(Image));
            barBgGO.transform.SetParent(hudCanvas.transform, false);
            var rtBg = barBgGO.GetComponent<RectTransform>();
            rtBg.anchorMin = new Vector2(0, 1);
            rtBg.anchorMax = new Vector2(0, 1);
            rtBg.pivot = new Vector2(0, 1);
            rtBg.anchoredPosition = new Vector2(30, -30);
            rtBg.sizeDelta = new Vector2(250, 24);
            barBgGO.GetComponent<Image>().color = new Color(0.12f, 0.02f, 0.02f, 0.9f);

            var barFillGO = new GameObject("HealthBar_Fill", typeof(RectTransform), typeof(Image));
            barFillGO.transform.SetParent(barBgGO.transform, false);
            var rtFill = barFillGO.GetComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.pivot = new Vector2(0, 0.5f);
            rtFill.sizeDelta = Vector2.zero;
            vitalityFill = barFillGO.GetComponent<Image>();
            vitalityFill.color = new Color(0.85f, 0.15f, 0.15f, 1.0f);
            vitalityFill.type = Image.Type.Filled;
            vitalityFill.fillMethod = Image.FillMethod.Horizontal;
            vitalityFill.fillAmount = 1.0f;

            // Vitality numeric text
            var textGO = new GameObject("HealthText", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(barBgGO.transform, false);
            var rtTxt = textGO.GetComponent<RectTransform>();
            rtTxt.anchorMin = Vector2.zero;
            rtTxt.anchorMax = Vector2.one;
            rtTxt.sizeDelta = Vector2.zero;
            vitalityText = textGO.GetComponent<Text>();
            vitalityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            vitalityText.fontSize = 12;
            vitalityText.alignment = TextAnchor.MiddleCenter;
            vitalityText.fontStyle = FontStyle.Bold;
            vitalityText.color = Color.white;
            vitalityText.text = "HP: 100 / 100";
        }

        // 2. Light Soul Counter
        if (soulCounterText == null)
        {
            var soulTextGO = new GameObject("SoulCounterText", typeof(RectTransform), typeof(Text));
            soulTextGO.transform.SetParent(hudCanvas.transform, false);
            var rtText = soulTextGO.GetComponent<RectTransform>();
            rtText.anchorMin = new Vector2(0, 1);
            rtText.anchorMax = new Vector2(0, 1);
            rtText.pivot = new Vector2(0, 1);
            rtText.anchoredPosition = new Vector2(30, -62);
            rtText.sizeDelta = new Vector2(300, 30);
            soulCounterText = soulTextGO.GetComponent<Text>();
            soulCounterText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            soulCounterText.fontSize = 16;
            soulCounterText.fontStyle = FontStyle.Bold;
            soulCounterText.color = new Color(1.0f, 0.88f, 0.35f, 1.0f);
            soulCounterText.text = "LIGHT SOULS: 0 / 5";
        }

        // 3. Boss Bar (Top Center)
        if (bossBarContainer == null)
        {
            bossBarContainer = new GameObject("BossBar_Container", typeof(RectTransform), typeof(Image));
            bossBarContainer.transform.SetParent(hudCanvas.transform, false);
            var rtBossBg = bossBarContainer.GetComponent<RectTransform>();
            rtBossBg.anchorMin = new Vector2(0.5f, 1);
            rtBossBg.anchorMax = new Vector2(0.5f, 1);
            rtBossBg.pivot = new Vector2(0.5f, 1);
            rtBossBg.anchoredPosition = new Vector2(0, -35);
            rtBossBg.sizeDelta = new Vector2(500, 24);
            bossBarContainer.GetComponent<Image>().color = new Color(0.15f, 0.05f, 0.05f, 0.85f);

            var bossFillGO = new GameObject("BossBar_Fill", typeof(RectTransform), typeof(Image));
            bossFillGO.transform.SetParent(bossBarContainer.transform, false);
            var rtBossFill = bossFillGO.GetComponent<RectTransform>();
            rtBossFill.anchorMin = Vector2.zero;
            rtBossFill.anchorMax = Vector2.one;
            rtBossFill.pivot = new Vector2(0, 0.5f);
            rtBossFill.sizeDelta = Vector2.zero;
            bossHealthFill = bossFillGO.GetComponent<Image>();
            bossHealthFill.color = new Color(0.95f, 0.2f, 0.1f, 1.0f);
            bossHealthFill.type = Image.Type.Filled;
            bossHealthFill.fillMethod = Image.FillMethod.Horizontal;
            bossHealthFill.fillAmount = 1.0f;

            var bossLabelGO = new GameObject("BossLabel", typeof(RectTransform), typeof(Text));
            bossLabelGO.transform.SetParent(bossBarContainer.transform, false);
            var rtBossLabel = bossLabelGO.GetComponent<RectTransform>();
            rtBossLabel.anchorMin = new Vector2(0.5f, 1);
            rtBossLabel.anchorMax = new Vector2(0.5f, 1);
            rtBossLabel.pivot = new Vector2(0.5f, 0);
            rtBossLabel.anchoredPosition = new Vector2(0, 4);
            rtBossLabel.sizeDelta = new Vector2(400, 22);
            var txt = bossLabelGO.GetComponent<Text>();
            txt.font = soulCounterText.font;
            txt.fontSize = 14;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.text = "KNIGHT OF VENGEANCE";

            bossBarContainer.SetActive(false);
        }

        // 4. High-Quality Rune Ascension Modal
        if (runeModalPanel == null)
        {
            runeModalPanel = new GameObject("RuneModal_HD_Panel", typeof(RectTransform), typeof(Image));
            runeModalPanel.transform.SetParent(hudCanvas.transform, false);
            var rtModal = runeModalPanel.GetComponent<RectTransform>();
            rtModal.anchorMin = Vector2.zero;
            rtModal.anchorMax = Vector2.one;
            rtModal.sizeDelta = Vector2.zero;
            runeModalPanel.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.92f);

            // Title
            var titleGO = new GameObject("Ascension_Title", typeof(RectTransform), typeof(Text));
            titleGO.transform.SetParent(runeModalPanel.transform, false);
            var rtTitle = titleGO.GetComponent<RectTransform>();
            rtTitle.anchorMin = new Vector2(0.5f, 0.85f);
            rtTitle.anchorMax = new Vector2(0.5f, 0.85f);
            rtTitle.sizeDelta = new Vector2(600, 50);
            var titleTxt = titleGO.GetComponent<Text>();
            titleTxt.font = soulCounterText.font;
            titleTxt.fontSize = 28;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(1.0f, 0.85f, 0.35f, 1.0f);
            titleTxt.text = "SOUL ASCENSION: CHOOSE A RUNE";

            // Card 1: Speed Rune
            CreateRuneCard(runeModalPanel.transform, new Vector2(-180, -30), "RUNE OF CELERITY", "+1.5 Movement Speed & Agile Dash", speedRuneSprite, SelectSpeedRune);

            // Card 2: Damage Rune
            CreateRuneCard(runeModalPanel.transform, new Vector2(180, -30), "RUNE OF DESTRUCTION", "+15 Katana Slash Damage", damageRuneSprite, SelectDamageRune);

            runeModalPanel.SetActive(false);
        }

        // 5. Victory Screen Banner
        if (victoryBannerPanel == null)
        {
            victoryBannerPanel = new GameObject("VictoryBanner_Panel", typeof(RectTransform), typeof(Image));
            victoryBannerPanel.transform.SetParent(hudCanvas.transform, false);
            var rtVic = victoryBannerPanel.GetComponent<RectTransform>();
            rtVic.anchorMin = Vector2.zero;
            rtVic.anchorMax = Vector2.one;
            rtVic.sizeDelta = Vector2.zero;
            victoryBannerPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.02f, 0.9f);

            var vicTitleGO = new GameObject("VicTitle", typeof(RectTransform), typeof(Text));
            vicTitleGO.transform.SetParent(victoryBannerPanel.transform, false);
            var rtVicTitle = vicTitleGO.GetComponent<RectTransform>();
            rtVicTitle.anchorMin = new Vector2(0.5f, 0.65f);
            rtVicTitle.anchorMax = new Vector2(0.5f, 0.65f);
            rtVicTitle.sizeDelta = new Vector2(800, 80);
            var txtVic = vicTitleGO.GetComponent<Text>();
            txtVic.font = soulCounterText.font;
            txtVic.fontSize = 44;
            txtVic.alignment = TextAnchor.MiddleCenter;
            txtVic.fontStyle = FontStyle.Bold;
            txtVic.color = new Color(1.0f, 0.84f, 0.25f, 1.0f);
            txtVic.text = "VICTORY ACHIEVED";

            var vicSubGO = new GameObject("VicSub", typeof(RectTransform), typeof(Text));
            vicSubGO.transform.SetParent(victoryBannerPanel.transform, false);
            var rtVicSub = vicSubGO.GetComponent<RectTransform>();
            rtVicSub.anchorMin = new Vector2(0.5f, 0.52f);
            rtVicSub.anchorMax = new Vector2(0.5f, 0.52f);
            rtVicSub.sizeDelta = new Vector2(800, 40);
            var txtSub = vicSubGO.GetComponent<Text>();
            txtSub.font = soulCounterText.font;
            txtSub.fontSize = 20;
            txtSub.alignment = TextAnchor.MiddleCenter;
            txtSub.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            txtSub.text = "Vengeance Claimed · Boss Slain";

            // Restart Button
            var btnRestartGO = new GameObject("Btn_Restart", typeof(RectTransform), typeof(Image), typeof(Button));
            btnRestartGO.transform.SetParent(victoryBannerPanel.transform, false);
            var rtBtn = btnRestartGO.GetComponent<RectTransform>();
            rtBtn.anchorMin = new Vector2(0.5f, 0.38f);
            rtBtn.anchorMax = new Vector2(0.5f, 0.38f);
            rtBtn.sizeDelta = new Vector2(240, 50);
            btnRestartGO.GetComponent<Image>().color = new Color(0.2f, 0.65f, 0.35f, 0.95f);
            var btn = btnRestartGO.GetComponent<Button>();
            btn.onClick.AddListener(() => {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            });

            var btnTxtGO = new GameObject("BtnText", typeof(RectTransform), typeof(Text));
            btnTxtGO.transform.SetParent(btnRestartGO.transform, false);
            var rtBtnTxt = btnTxtGO.GetComponent<RectTransform>();
            rtBtnTxt.anchorMin = Vector2.zero;
            rtBtnTxt.anchorMax = Vector2.one;
            rtBtnTxt.sizeDelta = Vector2.zero;
            var txtBtn = btnTxtGO.GetComponent<Text>();
            txtBtn.font = soulCounterText.font;
            txtBtn.fontSize = 18;
            txtBtn.alignment = TextAnchor.MiddleCenter;
            txtBtn.fontStyle = FontStyle.Bold;
            txtBtn.color = Color.white;
            txtBtn.text = "PLAY AGAIN";

            victoryBannerPanel.SetActive(false);
        }
    }

    private void CreateRuneCard(Transform parent, Vector2 pos, string title, string desc, Sprite icon, UnityEngine.Events.UnityAction onClick)
    {
        var cardGO = new GameObject("Card_" + title, typeof(RectTransform), typeof(Image), typeof(Button));
        cardGO.transform.SetParent(parent, false);
        var rtCard = cardGO.GetComponent<RectTransform>();
        rtCard.anchoredPosition = pos;
        rtCard.sizeDelta = new Vector2(250, 360);

        var imgCard = cardGO.GetComponent<Image>();
        if (cardFrameSprite != null)
        {
            imgCard.sprite = cardFrameSprite;
            imgCard.color = Color.white;
        }
        else
        {
            imgCard.color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        }

        var btn = cardGO.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        // Icon
        if (icon != null)
        {
            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(cardGO.transform, false);
            var rtIcon = iconGO.GetComponent<RectTransform>();
            rtIcon.anchoredPosition = new Vector2(0, 50);
            rtIcon.sizeDelta = new Vector2(140, 140);
            var imgIcon = iconGO.GetComponent<Image>();
            imgIcon.sprite = icon;
            imgIcon.preserveAspect = true;
        }

        // Title
        var titleGO = new GameObject("CardTitle", typeof(RectTransform), typeof(Text));
        titleGO.transform.SetParent(cardGO.transform, false);
        var rtTitle = titleGO.GetComponent<RectTransform>();
        rtTitle.anchoredPosition = new Vector2(0, -50);
        rtTitle.sizeDelta = new Vector2(230, 40);
        var txtTitle = titleGO.GetComponent<Text>();
        txtTitle.font = soulCounterText.font;
        txtTitle.fontSize = 15;
        txtTitle.alignment = TextAnchor.MiddleCenter;
        txtTitle.fontStyle = FontStyle.Bold;
        txtTitle.color = new Color(1.0f, 0.9f, 0.4f, 1.0f);
        txtTitle.text = title;

        // Desc
        var descGO = new GameObject("CardDesc", typeof(RectTransform), typeof(Text));
        descGO.transform.SetParent(cardGO.transform, false);
        var rtDesc = descGO.GetComponent<RectTransform>();
        rtDesc.anchoredPosition = new Vector2(0, -110);
        rtDesc.sizeDelta = new Vector2(220, 60);
        var txtDesc = descGO.GetComponent<Text>();
        txtDesc.font = soulCounterText.font;
        txtDesc.fontSize = 12;
        txtDesc.alignment = TextAnchor.MiddleCenter;
        txtDesc.color = new Color(0.85f, 0.88f, 0.92f, 1.0f);
        txtDesc.text = desc;
    }

    private void OnGUI()
    {
        if (!showDebugSoundboard)
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(10, Screen.height - 25, 380, 20), "[F1] Audio/UI Soundboard | [R] Rune Modal | [K] +1 Soul");
            return;
        }

        GUI.Box(new Rect(10, 10, 240, 290), "Tech/UI Art: Manthan");
        if (GUI.Button(new Rect(20, 35, 220, 25), "Toggle Rune Modal (R)")) ToggleRuneModal();
        if (GUI.Button(new Rect(20, 65, 220, 25), "+1 Soul Charge (K)"))
        {
            if (playerLight != null) playerLight.AbsorbLight(1);
        }
        if (GUI.Button(new Rect(20, 95, 220, 25), "Toggle Boss Bar (B)")) ToggleBossBar();
        if (GUI.Button(new Rect(20, 125, 220, 25), "Play Sword Swing")) VendettaAudioManager.Instance?.PlaySwordSwing();
        if (GUI.Button(new Rect(20, 155, 220, 25), "Play Sword Hit")) VendettaAudioManager.Instance?.PlaySwordHit();
        if (GUI.Button(new Rect(20, 185, 220, 25), "Play Player Hurt")) VendettaAudioManager.Instance?.PlayPlayerHurt();
        if (GUI.Button(new Rect(20, 215, 220, 25), "Play Soul Pickup")) VendettaAudioManager.Instance?.PlayPickupChime();
        if (GUI.Button(new Rect(20, 245, 220, 25), "Simulate Victory")) ShowVictoryScreen();
    }
}
