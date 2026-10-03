using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class VendettaUIManager : MonoBehaviour
{
    public static VendettaUIManager Instance { get; private set; }

    [Header("In-Game HUD")]
    public Canvas hudCanvas;
    public Image heartEmblemImage;
    public Image vitalityFill;
    public Text vitalityLabelText;
    public Image staminaFill;
    public Text staminaLabelText;
    public Image damageFlashOverlay;
    public Image bossHealthFill;
    public GameObject bossBarContainer;
    public Text soulCounterText;

    [Header("Ascension Rune Modal (Custom Designed)")]
    public GameObject ascensionModalPanel;
    public Sprite heartEmblemSprite;
    public Sprite cardFrameSprite;
    public Sprite speedRuneSprite;
    public Sprite damageRuneSprite;

    [Header("Victory Screen")]
    public GameObject victoryBannerPanel;

    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerLight playerLight;
    private BossHealth bossHealth;
    private UpgradeManager upgradeManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadRuneSprites();
    }

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerCombat = FindFirstObjectByType<PlayerCombat>();
        playerLight = FindFirstObjectByType<PlayerLight>();
        upgradeManager = FindFirstObjectByType<UpgradeManager>();
        bossHealth = FindFirstObjectByType<BossHealth>();

        GameEvents.OnBossDefeated += ShowVictoryScreen;

        BuildHUDAndAscensionModal();

        if (bossBarContainer != null) bossBarContainer.SetActive(false);
        if (victoryBannerPanel != null) victoryBannerPanel.SetActive(false);
        if (ascensionModalPanel != null) ascensionModalPanel.SetActive(false);

        // Hide teammate's placeholder canvas so our customized Ascension UI takes full effect
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
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerCombat == null) playerCombat = FindFirstObjectByType<PlayerCombat>();
        if (playerLight == null) playerLight = FindFirstObjectByType<PlayerLight>();
        if (upgradeManager == null) upgradeManager = FindFirstObjectByType<UpgradeManager>();
        if (bossHealth == null) bossHealth = FindFirstObjectByType<BossHealth>();

        // Update player health bar
        if (playerHealth != null && vitalityFill != null && playerHealth.maxHealth > 0)
        {
            float hpPct = Mathf.Clamp01(playerHealth.currentHealth / playerHealth.maxHealth);
            vitalityFill.rectTransform.anchorMax = new Vector2(hpPct, 1f);
            vitalityFill.fillAmount = hpPct;
        }

        // Update soul counter text
        if (soulCounterText != null)
        {
            int souls = playerLight != null ? playerLight.lightAmount : 0;
            int req = upgradeManager != null ? upgradeManager.requiredLight : 5;
            soulCounterText.text = "LIGHT SOULS: " + souls + " / " + req;
        }

        // Automatically trigger Ascension modal when upgrade state is reached
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Upgrade)
        {
            if (ascensionModalPanel != null && !ascensionModalPanel.activeSelf)
            {
                OpenAscensionModal();
            }
        }

        // Automatically trigger Victory screen when boss is defeated
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Victory)
        {
            if (victoryBannerPanel != null && !victoryBannerPanel.activeSelf)
            {
                ShowVictoryScreen();
            }
        }

        // Update boss health bar if active
        if (bossBarContainer != null && bossBarContainer.activeSelf && bossHealth != null && bossHealth.maxHealth > 0)
        {
            float bossPct = Mathf.Clamp01(bossHealth.currentHealth / bossHealth.maxHealth);
            if (bossHealthFill != null)
            {
                bossHealthFill.rectTransform.anchorMax = new Vector2(bossPct, 1f);
                bossHealthFill.fillAmount = bossPct;
            }
        }

        // Check if Ascension modal is open and handle keyboard hotkeys (1 / 2)
        if (ascensionModalPanel != null && ascensionModalPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SelectSwiftnessRune();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SelectFuryRune();
            }
        }
    }

    public void OpenAscensionModal()
    {
        if (ascensionModalPanel != null)
        {
            // Temporarily hide boss bar during modal selection
            if (bossBarContainer != null) bossBarContainer.SetActive(false);

            ascensionModalPanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            VendettaAudioManager.Instance?.PlayPickupChime();
        }
    }

    public void CloseAscensionModal()
    {
        if (ascensionModalPanel != null)
        {
            ascensionModalPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Upgrade)
        {
            GameManager.Instance.currentState = GameManager.GameState.Playing;
        }

        if (upgradeManager != null && upgradeManager.upgradeCanvas != null)
        {
            upgradeManager.upgradeCanvas.SetActive(false);
        }

        // If boss is active, restore boss bar
        if (bossHealth != null && bossHealth.gameObject.activeInHierarchy)
        {
            ShowBossBar(true);
        }
    }

    public void SelectSwiftnessRune()
    {
        if (playerMovement != null)
        {
            playerMovement.speed += 1.75f;
            Debug.Log("[Ascension] RUNE OF SWIFTNESS embodied! Speed: " + playerMovement.speed);
        }
        else if (upgradeManager != null)
        {
            upgradeManager.ChooseSpeedRune();
        }

        VendettaAudioManager.Instance?.PlaySwordSwing();
        VendettaAudioManager.Instance?.PlayPickupChime();
        CloseAscensionModal();
    }

    public void SelectFuryRune()
    {
        if (playerCombat != null)
        {
            playerCombat.attackDamage += 10f;
            Debug.Log("[Ascension] RUNE OF FURY embodied! Damage: " + playerCombat.attackDamage);
        }
        else if (upgradeManager != null)
        {
            upgradeManager.ChooseDamageRune();
        }

        VendettaAudioManager.Instance?.PlaySwordHit();
        VendettaAudioManager.Instance?.PlayPickupChime();
        CloseAscensionModal();
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
        if (ascensionModalPanel != null) ascensionModalPanel.SetActive(false);
        if (victoryBannerPanel != null)
        {
            victoryBannerPanel.transform.SetAsLastSibling();
            victoryBannerPanel.SetActive(true);
        }
        ShowBossBar(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        VendettaAudioManager.Instance?.PlayVictory();
    }

    public void ShowBossBar(bool show)
    {
        if (bossBarContainer != null) bossBarContainer.SetActive(show);
    }

    private void LoadRuneSprites()
    {
        if (heartEmblemSprite == null) heartEmblemSprite = LoadSpriteDirect("UI/T_HUD_Heart_Emblem.png");
        if (cardFrameSprite == null) cardFrameSprite = LoadSpriteDirect("UI/T_Card_Frame_HD.png");
        if (speedRuneSprite == null) speedRuneSprite = LoadSpriteDirect("UI/T_Rune_Speed_HD.png");
        if (damageRuneSprite == null) damageRuneSprite = LoadSpriteDirect("UI/T_Rune_Damage_HD.png");
    }

    private Sprite LoadSpriteDirect(string relativePath)
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, relativePath);
            if (File.Exists(fullPath))
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[VendettaUI] Could not load sprite from " + relativePath + ": " + e.Message);
        }
        return null;
    }

    private void BuildHUDAndAscensionModal()
    {
        if (hudCanvas == null)
        {
            var cGO = new GameObject("Vendetta_HUD_Canvas");
            hudCanvas = cGO.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = cGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            cGO.AddComponent<GraphicRaycaster>();
        }

        // Screen damage flash overlay
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

        // ==========================================
        // TOP-LEFT: VITALITY & STAMINA HUD
        // ==========================================
        if (vitalityFill == null)
        {
            // 1. Diamond Heart Emblem Icon
            var emblemGO = new GameObject("HUD_Heart_Emblem", typeof(RectTransform), typeof(Image));
            emblemGO.transform.SetParent(hudCanvas.transform, false);
            var rtEmblem = emblemGO.GetComponent<RectTransform>();
            rtEmblem.anchorMin = new Vector2(0, 1);
            rtEmblem.anchorMax = new Vector2(0, 1);
            rtEmblem.pivot = new Vector2(0, 1);
            rtEmblem.anchoredPosition = new Vector2(24, -20);
            rtEmblem.sizeDelta = new Vector2(58, 60);
            heartEmblemImage = emblemGO.GetComponent<Image>();
            if (heartEmblemSprite != null)
            {
                heartEmblemImage.sprite = heartEmblemSprite;
                heartEmblemImage.preserveAspect = true;
            }
            else
            {
                heartEmblemImage.color = new Color(0.85f, 0.15f, 0.18f, 1f);
            }

            // 2. Vitality Text Label
            var lblGO = new GameObject("Label_Vitality", typeof(RectTransform), typeof(Text));
            lblGO.transform.SetParent(hudCanvas.transform, false);
            var rtLbl = lblGO.GetComponent<RectTransform>();
            rtLbl.anchorMin = new Vector2(0, 1);
            rtLbl.anchorMax = new Vector2(0, 1);
            rtLbl.pivot = new Vector2(0, 1);
            rtLbl.anchoredPosition = new Vector2(92, -16);
            rtLbl.sizeDelta = new Vector2(120, 16);
            vitalityLabelText = lblGO.GetComponent<Text>();
            vitalityLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vitalityLabelText.fontSize = 11;
            vitalityLabelText.fontStyle = FontStyle.Bold;
            vitalityLabelText.color = new Color(0.88f, 0.32f, 0.32f, 1f);
            vitalityLabelText.text = "VITALITY";

            // 3. Red Health Bar Frame & Fill
            var hpBgGO = new GameObject("Vitality_BG", typeof(RectTransform), typeof(Image));
            hpBgGO.transform.SetParent(hudCanvas.transform, false);
            var rtHpBg = hpBgGO.GetComponent<RectTransform>();
            rtHpBg.anchorMin = new Vector2(0, 1);
            rtHpBg.anchorMax = new Vector2(0, 1);
            rtHpBg.pivot = new Vector2(0, 1);
            rtHpBg.anchoredPosition = new Vector2(90, -32);
            rtHpBg.sizeDelta = new Vector2(260, 15);
            hpBgGO.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.10f, 0.90f);

            var fillGO = new GameObject("Vitality_Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(hpBgGO.transform, false);
            var rtFill = fillGO.GetComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.sizeDelta = new Vector2(-2, -2);
            rtFill.anchoredPosition = Vector2.zero;
            vitalityFill = fillGO.GetComponent<Image>();
            vitalityFill.color = new Color(0.82f, 0.20f, 0.20f, 1.0f);

            // 4. Green Stamina Bar Frame & Fill
            var stBgGO = new GameObject("Stamina_BG", typeof(RectTransform), typeof(Image));
            stBgGO.transform.SetParent(hudCanvas.transform, false);
            var rtStBg = stBgGO.GetComponent<RectTransform>();
            rtStBg.anchorMin = new Vector2(0, 1);
            rtStBg.anchorMax = new Vector2(0, 1);
            rtStBg.pivot = new Vector2(0, 1);
            rtStBg.anchoredPosition = new Vector2(90, -50);
            rtStBg.sizeDelta = new Vector2(210, 13);
            stBgGO.GetComponent<Image>().color = new Color(0.06f, 0.09f, 0.07f, 0.90f);

            var stFillGO = new GameObject("Stamina_Fill", typeof(RectTransform), typeof(Image));
            stFillGO.transform.SetParent(stBgGO.transform, false);
            var rtStFill = stFillGO.GetComponent<RectTransform>();
            rtStFill.anchorMin = Vector2.zero;
            rtStFill.anchorMax = Vector2.one;
            rtStFill.sizeDelta = new Vector2(-2, -2);
            rtStFill.anchoredPosition = Vector2.zero;
            staminaFill = stFillGO.GetComponent<Image>();
            staminaFill.color = new Color(0.24f, 0.65f, 0.32f, 1.0f);

            // 5. Stamina Text Label
            var stLblGO = new GameObject("Label_Stamina", typeof(RectTransform), typeof(Text));
            stLblGO.transform.SetParent(hudCanvas.transform, false);
            var rtStLbl = stLblGO.GetComponent<RectTransform>();
            rtStLbl.anchorMin = new Vector2(0, 1);
            rtStLbl.anchorMax = new Vector2(0, 1);
            rtStLbl.pivot = new Vector2(0, 1);
            rtStLbl.anchoredPosition = new Vector2(92, -67);
            rtStLbl.sizeDelta = new Vector2(120, 16);
            staminaLabelText = stLblGO.GetComponent<Text>();
            staminaLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            staminaLabelText.fontSize = 11;
            staminaLabelText.fontStyle = FontStyle.Bold;
            staminaLabelText.color = new Color(0.48f, 0.78f, 0.52f, 1f);
            staminaLabelText.text = "STAMINA";
        }

        // Soul Counter Text
        if (soulCounterText == null)
        {
            var soulGO = new GameObject("SoulCounterText", typeof(RectTransform), typeof(Text));
            soulGO.transform.SetParent(hudCanvas.transform, false);
            var rtSoul = soulGO.GetComponent<RectTransform>();
            rtSoul.anchorMin = new Vector2(0, 1);
            rtSoul.anchorMax = new Vector2(0, 1);
            rtSoul.pivot = new Vector2(0, 1);
            rtSoul.anchoredPosition = new Vector2(25, -96);
            rtSoul.sizeDelta = new Vector2(260, 24);
            soulCounterText = soulGO.GetComponent<Text>();
            soulCounterText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            soulCounterText.fontSize = 13;
            soulCounterText.fontStyle = FontStyle.Bold;
            soulCounterText.color = new Color(0.92f, 0.82f, 0.45f, 1f);
            soulCounterText.text = "LIGHT SOULS: 0 / 5";
        }

        // ==========================================
        // TOP-CENTER: BOSS HEALTH BAR (Dead Center, Never Overlaps HUD)
        // ==========================================
        if (bossBarContainer == null)
        {
            bossBarContainer = new GameObject("BossBar_Container", typeof(RectTransform), typeof(Image));
            bossBarContainer.transform.SetParent(hudCanvas.transform, false);
            var rtBossBg = bossBarContainer.GetComponent<RectTransform>();
            rtBossBg.anchorMin = new Vector2(0.5f, 1f);
            rtBossBg.anchorMax = new Vector2(0.5f, 1f);
            rtBossBg.pivot = new Vector2(0.5f, 1f);
            rtBossBg.anchoredPosition = new Vector2(0, -30);
            rtBossBg.sizeDelta = new Vector2(560, 24);
            bossBarContainer.GetComponent<Image>().color = new Color(0.08f, 0.05f, 0.05f, 0.90f);

            var fillBossGO = new GameObject("BossFill", typeof(RectTransform), typeof(Image));
            fillBossGO.transform.SetParent(bossBarContainer.transform, false);
            var rtBossFill = fillBossGO.GetComponent<RectTransform>();
            rtBossFill.anchorMin = Vector2.zero;
            rtBossFill.anchorMax = Vector2.one;
            rtBossFill.sizeDelta = new Vector2(-4, -4);
            rtBossFill.anchoredPosition = Vector2.zero;
            bossHealthFill = fillBossGO.GetComponent<Image>();
            bossHealthFill.color = new Color(0.78f, 0.12f, 0.15f, 1.0f);

            var bossLblGO = new GameObject("BossLabel", typeof(RectTransform), typeof(Text));
            bossLblGO.transform.SetParent(bossBarContainer.transform, false);
            var rtBossLabel = bossLblGO.GetComponent<RectTransform>();
            rtBossLabel.anchorMin = Vector2.zero;
            rtBossLabel.anchorMax = Vector2.one;
            rtBossLabel.sizeDelta = Vector2.zero;
            var txtBoss = bossLblGO.GetComponent<Text>();
            txtBoss.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtBoss.fontSize = 12;
            txtBoss.alignment = TextAnchor.MiddleCenter;
            txtBoss.fontStyle = FontStyle.Bold;
            txtBoss.color = new Color(1f, 0.85f, 0.5f, 1f);
            txtBoss.text = "ANCIENT CORRUPTED KNIGHT";
            bossBarContainer.SetActive(false);
        }

        // ==========================================
        // ASCENSION : EMBODY YOUR RUNE MODAL (Properly Anchored & Scaled)
        // ==========================================
        if (ascensionModalPanel == null)
        {
            ascensionModalPanel = new GameObject("AscensionModal_Panel", typeof(RectTransform), typeof(Image));
            ascensionModalPanel.transform.SetParent(hudCanvas.transform, false);
            var rtModal = ascensionModalPanel.GetComponent<RectTransform>();
            rtModal.anchorMin = Vector2.zero;
            rtModal.anchorMax = Vector2.one;
            rtModal.sizeDelta = Vector2.zero;
            // Clean dark translucent vignette tint
            ascensionModalPanel.GetComponent<Image>().color = new Color(0.015f, 0.015f, 0.022f, 0.88f);

            // 1. TOP HEADER: Anchored to TOP-CENTER (y = -35)
            var headerGO = new GameObject("Header_Container", typeof(RectTransform));
            headerGO.transform.SetParent(ascensionModalPanel.transform, false);
            var rtHeader = headerGO.GetComponent<RectTransform>();
            rtHeader.anchorMin = new Vector2(0.5f, 1f);
            rtHeader.anchorMax = new Vector2(0.5f, 1f);
            rtHeader.pivot = new Vector2(0.5f, 1f);
            rtHeader.anchoredPosition = new Vector2(0, -30);
            rtHeader.sizeDelta = new Vector2(900, 70);

            var titleGO = new GameObject("AscensionTitle", typeof(RectTransform), typeof(Text));
            titleGO.transform.SetParent(headerGO.transform, false);
            var rtTitle = titleGO.GetComponent<RectTransform>();
            rtTitle.anchorMin = new Vector2(0.5f, 1f);
            rtTitle.anchorMax = new Vector2(0.5f, 1f);
            rtTitle.pivot = new Vector2(0.5f, 1f);
            rtTitle.anchoredPosition = new Vector2(0, 0);
            rtTitle.sizeDelta = new Vector2(900, 38);
            var txtTitle = titleGO.GetComponent<Text>();
            txtTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtTitle.fontSize = 21;
            txtTitle.alignment = TextAnchor.MiddleCenter;
            txtTitle.fontStyle = FontStyle.Bold;
            txtTitle.color = new Color(0.89f, 0.75f, 0.40f, 1f);
            txtTitle.text = "❖  ASCENSION : EMBODY YOUR RUNE  ❖";

            var subGO = new GameObject("AscensionSubtitle", typeof(RectTransform), typeof(Text));
            subGO.transform.SetParent(headerGO.transform, false);
            var rtSub = subGO.GetComponent<RectTransform>();
            rtSub.anchorMin = new Vector2(0.5f, 1f);
            rtSub.anchorMax = new Vector2(0.5f, 1f);
            rtSub.pivot = new Vector2(0.5f, 1f);
            rtSub.anchoredPosition = new Vector2(0, -36);
            rtSub.sizeDelta = new Vector2(900, 24);
            var txtSub = subGO.GetComponent<Text>();
            txtSub.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtSub.fontSize = 11;
            txtSub.alignment = TextAnchor.MiddleCenter;
            txtSub.color = new Color(0.72f, 0.72f, 0.75f, 1f);
            txtSub.text = "Five souls harvested. Infuse your blade with divine power to conquer the Titan Knight.";

            // 2. CENTER CARDS: Anchored to EXACT SCREEN CENTER
            var cardsGO = new GameObject("Cards_Container", typeof(RectTransform));
            cardsGO.transform.SetParent(ascensionModalPanel.transform, false);
            var rtCards = cardsGO.GetComponent<RectTransform>();
            rtCards.anchorMin = new Vector2(0.5f, 0.5f);
            rtCards.anchorMax = new Vector2(0.5f, 0.5f);
            rtCards.pivot = new Vector2(0.5f, 0.5f);
            rtCards.anchoredPosition = new Vector2(0, 5);
            rtCards.sizeDelta = new Vector2(740, 380);

            // Left Card: RUNE OF SWIFTNESS
            CreateAscensionCard(
                parent: cardsGO.transform,
                anchoredPos: new Vector2(-205, 0),
                title: "RUNE OF SWIFTNESS",
                titleColor: new Color(0.38f, 0.74f, 0.92f, 1f), // Cyan
                icon: speedRuneSprite,
                quote: "\"Gale-force fury awakens in your limbs. Strike like an untamed hurricane.\"",
                bulletPoints: "✦  +35% Movement Speed",
                bulletColor: new Color(0.42f, 0.80f, 0.95f, 1f),
                buttonLabel: "⬥  EMBODY (PRESS 1)  ⬥",
                onClick: SelectSwiftnessRune
            );

            // Right Card: RUNE OF FURY
            CreateAscensionCard(
                parent: cardsGO.transform,
                anchoredPos: new Vector2(205, 0),
                title: "RUNE OF FURY",
                titleColor: new Color(0.98f, 0.55f, 0.22f, 1f), // Orange
                icon: damageRuneSprite,
                quote: "\"Cleave through blackened armor. Unbridled flame ignites within the blade.\"",
                bulletPoints: "✦  +40% Heavy Slash Damage",
                bulletColor: new Color(0.98f, 0.60f, 0.35f, 1f),
                buttonLabel: "⬥  EMBODY (PRESS 2)  ⬥",
                onClick: SelectFuryRune
            );

            // 3. BOTTOM FOOTER: Anchored to BOTTOM-CENTER
            var footerGO = new GameObject("Footer_Container", typeof(RectTransform));
            footerGO.transform.SetParent(ascensionModalPanel.transform, false);
            var rtFooter = footerGO.GetComponent<RectTransform>();
            rtFooter.anchorMin = new Vector2(0.5f, 0f);
            rtFooter.anchorMax = new Vector2(0.5f, 0f);
            rtFooter.pivot = new Vector2(0.5f, 0f);
            rtFooter.anchoredPosition = new Vector2(0, 20);
            rtFooter.sizeDelta = new Vector2(600, 70);

            // 5 Soul Slots Indicator
            var soulSlotsGO = new GameObject("SoulSlots_Row", typeof(RectTransform));
            soulSlotsGO.transform.SetParent(footerGO.transform, false);
            var rtSlots = soulSlotsGO.GetComponent<RectTransform>();
            rtSlots.anchorMin = new Vector2(0.5f, 1f);
            rtSlots.anchorMax = new Vector2(0.5f, 1f);
            rtSlots.pivot = new Vector2(0.5f, 1f);
            rtSlots.anchoredPosition = new Vector2(0, 0);
            rtSlots.sizeDelta = new Vector2(220, 32);

            for (int i = 0; i < 5; i++)
            {
                var slot = new GameObject("Slot_" + (i + 1), typeof(RectTransform), typeof(Image));
                slot.transform.SetParent(soulSlotsGO.transform, false);
                var rtS = slot.GetComponent<RectTransform>();
                rtS.anchoredPosition = new Vector2((i - 2) * 40, 0);
                rtS.sizeDelta = new Vector2(30, 30);
                var imgS = slot.GetComponent<Image>();
                imgS.color = new Color(0.89f, 0.75f, 0.40f, 0.88f);
            }

            // Bottom Prompt Text
            var promptGO = new GameObject("PromptText", typeof(RectTransform), typeof(Text));
            promptGO.transform.SetParent(footerGO.transform, false);
            var rtPrompt = promptGO.GetComponent<RectTransform>();
            rtPrompt.anchorMin = new Vector2(0.5f, 0f);
            rtPrompt.anchorMax = new Vector2(0.5f, 0f);
            rtPrompt.pivot = new Vector2(0.5f, 0f);
            rtPrompt.anchoredPosition = new Vector2(0, 4);
            rtPrompt.sizeDelta = new Vector2(600, 20);
            var txtPrompt = promptGO.GetComponent<Text>();
            txtPrompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtPrompt.fontSize = 11;
            txtPrompt.alignment = TextAnchor.MiddleCenter;
            txtPrompt.color = new Color(0.60f, 0.60f, 0.65f, 1f);
            txtPrompt.text = "[ Click either Card or Press 1 / 2 on Keyboard to Embody ]";

            ascensionModalPanel.SetActive(false);
        }

        // Victory Screen Banner
        if (victoryBannerPanel == null)
        {
            victoryBannerPanel = new GameObject("VictoryBanner_Panel", typeof(RectTransform), typeof(Image));
            victoryBannerPanel.transform.SetParent(hudCanvas.transform, false);
            var rtVic = victoryBannerPanel.GetComponent<RectTransform>();
            rtVic.anchorMin = Vector2.zero;
            rtVic.anchorMax = Vector2.one;
            rtVic.sizeDelta = Vector2.zero;
            victoryBannerPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.02f, 0.92f);

            var vicTitleGO = new GameObject("VicTitle", typeof(RectTransform), typeof(Text));
            vicTitleGO.transform.SetParent(victoryBannerPanel.transform, false);
            var rtVicTitle = vicTitleGO.GetComponent<RectTransform>();
            rtVicTitle.anchorMin = new Vector2(0.5f, 0.65f);
            rtVicTitle.anchorMax = new Vector2(0.5f, 0.65f);
            rtVicTitle.sizeDelta = new Vector2(800, 80);
            var txtVic = vicTitleGO.GetComponent<Text>();
            txtVic.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            txtSub.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtSub.fontSize = 18;
            txtSub.alignment = TextAnchor.MiddleCenter;
            txtSub.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            txtSub.text = "Vengeance Claimed - Boss Slain";

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
            txtBtn.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtBtn.fontSize = 18;
            txtBtn.alignment = TextAnchor.MiddleCenter;
            txtBtn.fontStyle = FontStyle.Bold;
            txtBtn.color = Color.white;
            txtBtn.text = "PLAY AGAIN";

            victoryBannerPanel.SetActive(false);
        }
    }

    private void CreateAscensionCard(
        Transform parent,
        Vector2 anchoredPos,
        string title,
        Color titleColor,
        Sprite icon,
        string quote,
        string bulletPoints,
        Color bulletColor,
        string buttonLabel,
        UnityEngine.Events.UnityAction onClick)
    {
        var cardGO = new GameObject("Card_" + title.Replace(" ", "_"), typeof(RectTransform), typeof(Image), typeof(Button));
        cardGO.transform.SetParent(parent, false);
        var rtCard = cardGO.GetComponent<RectTransform>();
        rtCard.anchoredPosition = anchoredPos;
        rtCard.sizeDelta = new Vector2(270, 370);

        var imgCard = cardGO.GetComponent<Image>();
        if (cardFrameSprite != null)
        {
            imgCard.sprite = cardFrameSprite;
            imgCard.color = Color.white;
        }
        else
        {
            imgCard.color = new Color(0.04f, 0.05f, 0.07f, 0.96f);
        }

        var btn = cardGO.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        // Card Title
        var titleGO = new GameObject("CardTitle", typeof(RectTransform), typeof(Text));
        titleGO.transform.SetParent(cardGO.transform, false);
        var rtTitle = titleGO.GetComponent<RectTransform>();
        rtTitle.anchoredPosition = new Vector2(0, 150);
        rtTitle.sizeDelta = new Vector2(250, 30);
        var txtTitle = titleGO.GetComponent<Text>();
        txtTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtTitle.fontSize = 15;
        txtTitle.alignment = TextAnchor.MiddleCenter;
        txtTitle.fontStyle = FontStyle.Bold;
        txtTitle.color = titleColor;
        txtTitle.text = title;

        // Card Icon
        if (icon != null)
        {
            var iconGO = new GameObject("CardIcon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(cardGO.transform, false);
            var rtIcon = iconGO.GetComponent<RectTransform>();
            rtIcon.anchoredPosition = new Vector2(0, 75);
            rtIcon.sizeDelta = new Vector2(95, 95);
            var imgIcon = iconGO.GetComponent<Image>();
            imgIcon.sprite = icon;
            imgIcon.preserveAspect = true;
        }

        // Flavor Quote
        var quoteGO = new GameObject("CardQuote", typeof(RectTransform), typeof(Text));
        quoteGO.transform.SetParent(cardGO.transform, false);
        var rtQuote = quoteGO.GetComponent<RectTransform>();
        rtQuote.anchoredPosition = new Vector2(0, 0);
        rtQuote.sizeDelta = new Vector2(240, 40);
        var txtQuote = quoteGO.GetComponent<Text>();
        txtQuote.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtQuote.fontSize = 9;
        txtQuote.alignment = TextAnchor.MiddleCenter;
        txtQuote.fontStyle = FontStyle.Italic;
        txtQuote.color = new Color(0.72f, 0.74f, 0.78f, 1f);
        txtQuote.text = quote;

        // Perk Bullet Points (Single focused perk)
        var bulletGO = new GameObject("CardBullets", typeof(RectTransform), typeof(Text));
        bulletGO.transform.SetParent(cardGO.transform, false);
        var rtBullet = bulletGO.GetComponent<RectTransform>();
        rtBullet.anchoredPosition = new Vector2(0, -65);
        rtBullet.sizeDelta = new Vector2(240, 35);
        var txtBullet = bulletGO.GetComponent<Text>();
        txtBullet.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtBullet.fontSize = 13;
        txtBullet.alignment = TextAnchor.MiddleCenter;
        txtBullet.fontStyle = FontStyle.Bold;
        txtBullet.color = bulletColor;
        txtBullet.text = bulletPoints;

        // Embody Button (Bottom)
        var btnGO = new GameObject("Btn_Embody", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(cardGO.transform, false);
        var rtBtn = btnGO.GetComponent<RectTransform>();
        rtBtn.anchoredPosition = new Vector2(0, -145);
        rtBtn.sizeDelta = new Vector2(210, 36);
        btnGO.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

        var innerBtn = btnGO.GetComponent<Button>();
        innerBtn.onClick.AddListener(onClick);

        var btnTxtGO = new GameObject("BtnText", typeof(RectTransform), typeof(Text));
        btnTxtGO.transform.SetParent(btnGO.transform, false);
        var rtBtnTxt = btnTxtGO.GetComponent<RectTransform>();
        rtBtnTxt.anchorMin = Vector2.zero;
        rtBtnTxt.anchorMax = Vector2.one;
        rtBtnTxt.sizeDelta = Vector2.zero;
        var txtBtn = btnTxtGO.GetComponent<Text>();
        txtBtn.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtBtn.fontSize = 11;
        txtBtn.alignment = TextAnchor.MiddleCenter;
        txtBtn.fontStyle = FontStyle.Bold;
        txtBtn.color = new Color(0.89f, 0.75f, 0.40f, 1f);
        txtBtn.text = buttonLabel;
    }
}
