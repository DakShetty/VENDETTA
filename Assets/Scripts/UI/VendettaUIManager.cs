using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Rune Modal Reference")]
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerLight = FindFirstObjectByType<PlayerLight>();
        upgradeManager = FindFirstObjectByType<UpgradeManager>();
        bossHealth = FindFirstObjectByType<BossHealth>();

        GameEvents.OnBossDefeated += ShowVictoryScreen;

        BuildHUDIfMissing();

        if (bossBarContainer != null) bossBarContainer.SetActive(false);
        if (victoryBannerPanel != null) victoryBannerPanel.SetActive(false);
        if (runeModalPanel != null) runeModalPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        GameEvents.OnBossDefeated -= ShowVictoryScreen;
    }

    private void Update()
    {
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerLight == null) playerLight = FindFirstObjectByType<PlayerLight>();
        if (upgradeManager == null) upgradeManager = FindFirstObjectByType<UpgradeManager>();
        if (bossHealth == null) bossHealth = FindFirstObjectByType<BossHealth>();

        // Update player health bar
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

    public void ShowBossBar(bool show)
    {
        if (bossBarContainer != null) bossBarContainer.SetActive(show);
    }

    private void BuildHUDIfMissing()
    {
        if (hudCanvas == null)
        {
            var cGO = new GameObject("Vendetta_HUD_Canvas");
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
            rtBg.sizeDelta = new Vector2(280, 26);
            barBgGO.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.85f);

            var fillGO = new GameObject("HealthBar_Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(barBgGO.transform, false);
            var rtFill = fillGO.GetComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.sizeDelta = new Vector2(-4, -4);
            rtFill.anchoredPosition = Vector2.zero;
            vitalityFill = fillGO.GetComponent<Image>();
            vitalityFill.color = new Color(0.85f, 0.15f, 0.18f, 1.0f);

            var hpTxtGO = new GameObject("HealthText", typeof(RectTransform), typeof(Text));
            hpTxtGO.transform.SetParent(barBgGO.transform, false);
            var rtHpTxt = hpTxtGO.GetComponent<RectTransform>();
            rtHpTxt.anchorMin = Vector2.zero;
            rtHpTxt.anchorMax = Vector2.one;
            rtHpTxt.sizeDelta = Vector2.zero;
            vitalityText = hpTxtGO.GetComponent<Text>();
            vitalityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vitalityText.fontSize = 13;
            vitalityText.alignment = TextAnchor.MiddleCenter;
            vitalityText.fontStyle = FontStyle.Bold;
            vitalityText.color = Color.white;
            vitalityText.text = "HP: 100 / 100";
        }

        // 2. Soul Counter HUD
        if (soulCounterText == null)
        {
            var soulGO = new GameObject("SoulCounterText", typeof(RectTransform), typeof(Text));
            soulGO.transform.SetParent(hudCanvas.transform, false);
            var rtSoul = soulGO.GetComponent<RectTransform>();
            rtSoul.anchorMin = new Vector2(0, 1);
            rtSoul.anchorMax = new Vector2(0, 1);
            rtSoul.pivot = new Vector2(0, 1);
            rtSoul.anchoredPosition = new Vector2(30, -65);
            rtSoul.sizeDelta = new Vector2(320, 28);
            soulCounterText = soulGO.GetComponent<Text>();
            soulCounterText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            soulCounterText.fontSize = 14;
            soulCounterText.fontStyle = FontStyle.Bold;
            soulCounterText.alignment = TextAnchor.MiddleLeft;
            soulCounterText.color = new Color(0.95f, 0.85f, 0.4f, 1.0f);
            soulCounterText.text = "LIGHT SOULS: 0 / 5";
        }

        // 3. Boss Health Bar (Hidden initially)
        if (bossBarContainer == null)
        {
            bossBarContainer = new GameObject("BossBar_Container", typeof(RectTransform), typeof(Image));
            bossBarContainer.transform.SetParent(hudCanvas.transform, false);
            var rtBossBg = bossBarContainer.GetComponent<RectTransform>();
            rtBossBg.anchorMin = new Vector2(0.5f, 1f);
            rtBossBg.anchorMax = new Vector2(0.5f, 1f);
            rtBossBg.pivot = new Vector2(0.5f, 1f);
            rtBossBg.anchoredPosition = new Vector2(0, -25);
            rtBossBg.sizeDelta = new Vector2(500, 24);
            bossBarContainer.GetComponent<Image>().color = new Color(0.08f, 0.05f, 0.05f, 0.88f);

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

        // 4. Clean Fallback Rune Panel (only used if UpgradeCanvas is missing from scene)
        if (upgradeManager != null && upgradeManager.upgradeCanvas == null && runeModalPanel == null)
        {
            runeModalPanel = new GameObject("RuneModal_Fallback", typeof(RectTransform), typeof(Image));
            runeModalPanel.transform.SetParent(hudCanvas.transform, false);
            var rtModal = runeModalPanel.GetComponent<RectTransform>();
            rtModal.anchorMin = Vector2.zero;
            rtModal.anchorMax = Vector2.one;
            rtModal.sizeDelta = Vector2.zero;
            runeModalPanel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.07f, 0.92f);

            var titleGO = new GameObject("RuneTitle", typeof(RectTransform), typeof(Text));
            titleGO.transform.SetParent(runeModalPanel.transform, false);
            var rtTitle = titleGO.GetComponent<RectTransform>();
            rtTitle.anchorMin = new Vector2(0.5f, 0.72f);
            rtTitle.anchorMax = new Vector2(0.5f, 0.72f);
            rtTitle.sizeDelta = new Vector2(600, 60);
            var titleTxt = titleGO.GetComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 32;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(1.0f, 0.85f, 0.3f, 1.0f);
            titleTxt.text = "CHOOSE YOUR RUNE";

            CreateSimpleRuneBtn(runeModalPanel.transform, new Vector2(-140, 0), "SPEED RUNE", () => {
                upgradeManager?.ChooseSpeedRune();
                runeModalPanel.SetActive(false);
            });

            CreateSimpleRuneBtn(runeModalPanel.transform, new Vector2(140, 0), "DAMAGE RUNE", () => {
                upgradeManager?.ChooseDamageRune();
                runeModalPanel.SetActive(false);
            });

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
            txtSub.fontSize = 20;
            txtSub.alignment = TextAnchor.MiddleCenter;
            txtSub.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            txtSub.text = "Boss Slain";

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

    private void CreateSimpleRuneBtn(Transform parent, Vector2 pos, string label, UnityEngine.Events.UnityAction onClick)
    {
        var btnGO = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        var rtBtn = btnGO.GetComponent<RectTransform>();
        rtBtn.anchoredPosition = pos;
        rtBtn.sizeDelta = new Vector2(220, 60);
        btnGO.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.30f, 0.95f);

        var btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        var txtGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtGO.transform.SetParent(btnGO.transform, false);
        var rtTxt = txtGO.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.sizeDelta = Vector2.zero;
        var txt = txtGO.GetComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 16;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontStyle = FontStyle.Bold;
        txt.color = Color.white;
        txt.text = label;
    }
}
