using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;

    [SerializeField] Human humanPrefab;
    [SerializeField] HumanHarpooner humanHarpoonerPrefab;
    [SerializeField] Vector2 spawnIntervalTime = new Vector2(1f, 2f);
    [SerializeField] int maxHumanCount = 20;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Transform[] targetPoints;

    [Header("Water Pollution")]
    [SerializeField] Renderer[] waterRenderers;
    [SerializeField] Color pollutionColor;
    Color waterBaseColor;

    [Header("Oxygen Settings")]
    [SerializeField] float oxygenDecrement = 0.025f;
    [SerializeField] float oxygenIncrement = 0.1f;

    [Header("UI")]
    [SerializeField] Slider pollutionSlider;
    [SerializeField] Text pollutionText;

    [Space]
    [SerializeField] Slider oxygenSlider;
    [SerializeField] Text oxygenText;
    [SerializeField] Text oxygenLossText;

    [Space]
    [SerializeField] Slider[] playerOxygenSliders;

    [Space]
    [SerializeField] Text killCounterText;
    [SerializeField] Image underWaterOverlay;
    Color underWaterDefaultColor;
    Color underWaterAlphaColor;

    [SerializeField] Image hurtOverlay;
    Color hurtOverlayColor;
    Color hurtOverlayAlpha;

    [Space]
    [SerializeField] GameObject exitGameScreen;

    [Header("UI GameOver")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] Text scoreText;
    [SerializeField] Text bestScoreText;
    [SerializeField] Text retryButtonText;

    float spawnTime;
    float spawnCount;
    float currentSpawnTimer;

    int killCount;
    int killCountLastFrame;

    bool hitTutorialMessagePlayed;

    PlayerController playerController;
    Freezer frameFreezer;

    public float Pollution { get; private set; }
    private float pollutionLastFrame;

    public float Oxygen { get; private set; }
    private float oxygenLastFrame;

    public bool GameOver { get; private set; }

    private string[] retryTextVariants = new string[]
    {
        "Again!",
        "Retry!",
        "More humans!",
        "Fish 'n' Chops",
        "Hook 'em!",
        "Fried human."
    };

    private void Awake()
    {
        // Singleton setup.
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        // General setup.
        waterBaseColor = waterRenderers[0].material.color;

        playerController = FindObjectOfType<PlayerController>();
        frameFreezer = FindObjectOfType<Freezer>();

        underWaterDefaultColor = underWaterOverlay.color;
        underWaterAlphaColor = underWaterDefaultColor;
        underWaterAlphaColor.a = 0;

        hurtOverlayColor = hurtOverlay.color;
        hurtOverlayAlpha = hurtOverlayColor;
        hurtOverlayAlpha.a = 0;
        hurtOverlay.color = hurtOverlayAlpha;
        oxygenLossText.enabled = false;
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        // Gameplay and visuals.
        EnemySpawnUpdate();
        WaterColorUpdate();
        OxygenUpdate();

        // UI
        PollutionUIUpdate();
        OxygenUIUpdate();
        KillCountUIUpdate();
        GameOverScreenUIUpdate();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleExitWindow();
        }

        // Loose check.
        if (Oxygen <= 0 && !GameOver) // GAME OVER
        {
            LooseGame();
        }

        // Hurt overlay alpha lerp.
        hurtOverlay.color = Color.Lerp(hurtOverlay.color, hurtOverlayAlpha, Time.deltaTime * 7);
    }

    public void StartGame()
    {
        Oxygen = 1;
        Pollution = 0;
        killCount = 0;

        GameOver = false;
        Time.timeScale = 1;

        exitGameScreen.SetActive(false);
    }

    public void LooseGame()
    {
        GameOver = true;
        Time.timeScale = 0;

        // UI
        gameOverScreen.transform.localScale = Vector3.zero;

        retryButtonText.text = retryTextVariants[Random.Range(0, retryTextVariants.Length)];

        scoreText.text = killCount.ToString();
        int bestScore = PlayerPrefs.GetInt("bestscore", 0);
        if (killCount > bestScore)
        {
            PlayerPrefs.SetInt("bestscore", killCount);
            bestScore = killCount;
            Debug.Log("<b>New highscore!</b>");
        }

        bestScoreText.text = bestScore.ToString();
        hitTutorialMessagePlayed = false;
    }

    public void AddPollution(float value)
    {
        Pollution += value;
        Pollution = Mathf.Clamp01(Pollution);
    }

    public void AddOxygen(float value)
    {
        Oxygen += value;
        Oxygen = Mathf.Clamp01(Oxygen);
    }

    public void DecreaseSpawnCount()
    {
        spawnCount--;
        spawnCount = Mathf.Max(spawnCount, 0);
    }

    public void FreezeFrame()
    {
        frameFreezer.Freeze();
    }

    public void AddKillCount(int value = 1)
    {
        killCount += value;
    }

    public void ReloadGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void CancelExitGame()
    {
        exitGameScreen.SetActive(false);

        if (exitGameScreen.activeSelf)
        {
            Time.timeScale = 0;
        }
        else if (!exitGameScreen.activeSelf && !GameOver)
        {
            Time.timeScale = 1;
        }
    }

    /// <summary>
    /// A function that flashes a set UI overlay image. This overlay is lerped back to "0" alpha every update.
    /// </summary>
    public void HurtFlash()
    {
        hurtOverlay.color = hurtOverlayColor;
        EZCameraShake.CameraShaker.Instance.ShakeOnce(5, 4, 0, 0.2f);
    }

    /// <summary>
    /// A function that flashes a UI text warning that the player was hit.
    /// </summary>
    public void ShowHitWarning()
    {
        if (!hitTutorialMessagePlayed)
        {
            StartCoroutine(DoShowHitMessage(3f));
            hitTutorialMessagePlayed = true;
        }      
    }

    private IEnumerator DoShowHitMessage(float seconds)
    {
        oxygenLossText.enabled = true;
        yield return new WaitForSeconds(seconds);
        oxygenLossText.enabled = false;
    }

    /// <summary>
    /// Spawns enemies according to a timer and the current enemy count.
    /// </summary>
    private void EnemySpawnUpdate()
    {
        currentSpawnTimer += Time.deltaTime;
        if (currentSpawnTimer >= spawnTime && spawnCount < maxHumanCount)
        {
            // Unit type decision.
            bool isHarpooner = (Random.Range(0f, 1f) < 0.2f);

            // Spawnpoint decision.
            int pointID = Random.Range(0, spawnPoints.Length);

            if (isHarpooner)
            {
                // If it's a harpooner, spawn somewhere on the choosen ledge.
                Vector3 spawnPoint = Vector3.Lerp(spawnPoints[pointID].position, targetPoints[pointID].position, Random.Range(0f, 1f));
                Instantiate(humanHarpoonerPrefab, spawnPoint, Quaternion.identity);
            }
            else
            {
                // If it's a normal unit, spawn on the chosen spawn point.
                Human spawnedHuman = Instantiate(humanPrefab, spawnPoints[pointID].position, Quaternion.identity);
                spawnedHuman.InitHuman(spawnPoints[pointID], targetPoints[pointID]);
            }

            // Set cooldown to a new random value.
            spawnTime = Random.Range(spawnIntervalTime.x, spawnIntervalTime.y);

            // Reset timer and add to unit count.
            currentSpawnTimer = 0;
            spawnCount++;
        }
    }

    /// <summary>
    /// Updates the color of the water to match the amount of pollution.
    /// </summary>
    private void WaterColorUpdate()
    {
        // Water material colors.
        foreach (Renderer renderer in waterRenderers)
        {
            Color pollutionAlphaColor = pollutionColor;
            pollutionAlphaColor.a = renderer.material.color.a;
            renderer.material.color = Color.Lerp(waterBaseColor, pollutionAlphaColor, Pollution);
        }

        // UI Water overlay color.
        Color pollutionUIColor = pollutionColor;
        pollutionUIColor.a = underWaterDefaultColor.a;
        underWaterDefaultColor = Color.Lerp(underWaterDefaultColor, pollutionUIColor, Pollution);
    }

    /// <summary>
    /// Checks if the player is above or under water and manipulates the oxygen value (and UI elements) accordingly.
    /// </summary>
    private void OxygenUpdate()
    {
        if (playerController.AboveWater)
        {
            float oxygenChange = oxygenDecrement;
            if (Oxygen < 0.2f)
            {
                oxygenChange *= 0.5f;
            }

            AddOxygen(-(oxygenChange * Time.deltaTime));

            // UI
            underWaterOverlay.color = Color.Lerp(
                underWaterOverlay.color,
                underWaterAlphaColor,
                Time.unscaledDeltaTime * 7
            );
        }
        else
        {
            if (Pollution < 1)
            {
                AddOxygen(oxygenIncrement * Time.deltaTime * (1 - Pollution));
            }
            else
            {
                AddOxygen(-((oxygenDecrement / 1.5f) * Time.deltaTime));
            }

            // UI
            underWaterOverlay.color = Color.Lerp(
                underWaterOverlay.color,
                underWaterDefaultColor,
                Time.unscaledDeltaTime * 7
            );
        }
    }

    /// <summary>
    /// Updates the UI related to the pollution. 
    /// </summary>
    private void PollutionUIUpdate()
    {
        pollutionSlider.value = Pollution;
        pollutionText.text = Mathf.RoundToInt(Pollution * 100) + "%";
        pollutionText.transform.localScale = Vector3.Lerp(
            pollutionText.transform.localScale,
            Vector3.one,
            Time.deltaTime * 7
        );

        if (Pollution > pollutionLastFrame)
        {
            pollutionText.transform.localScale = Vector3.one * 1.25f;
        }

        pollutionLastFrame = Pollution;
    }

    /// <summary>
    /// Updates the UI related to the oxygen. 
    /// </summary>
    private void OxygenUIUpdate()
    {
        oxygenSlider.value = Oxygen;
        oxygenText.text = Mathf.RoundToInt(Oxygen * 100) + "%";
        oxygenText.transform.localScale = Vector3.Lerp(
            oxygenText.transform.localScale,
            Vector3.one,
            Time.deltaTime * 7
        );

        if (Oxygen > oxygenLastFrame)
        {
            oxygenText.transform.localScale = Vector3.one * 1.25f;
        }

        oxygenLastFrame = Oxygen;

        if (playerOxygenSliders.Length > 0)
        {
            foreach (Slider oxygenSlider in playerOxygenSliders)
            {
                oxygenSlider.value = Oxygen;
            }
        }
    }

    /// <summary>
    /// Updates the UI related to the kill counter. 
    /// </summary>
    private void KillCountUIUpdate()
    {
        killCounterText.text = killCount.ToString();
        killCounterText.transform.localScale = Vector3.Lerp(
            killCounterText.transform.localScale,
            Vector3.one,
            Time.deltaTime * 7
        );

        if (killCount > killCountLastFrame)
        {
            killCounterText.transform.localScale = Vector3.one * 1.5f;
        }

        killCountLastFrame = killCount;
    }

    /// <summary>
    /// Checks and updates the UI related to the game over screen. 
    /// </summary>
    private void GameOverScreenUIUpdate()
    {
        if (GameOver)
        {
            gameOverScreen.SetActive(true);
            gameOverScreen.transform.localScale = Vector3.Lerp(
                gameOverScreen.transform.localScale,
                Vector3.one,
                Time.unscaledDeltaTime * 7
            );
        }
        else
        {
            gameOverScreen.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles the "Do you want to exit" window (and pauses or unpauses the game).
    /// </summary>
    private void ToggleExitWindow()
    {
        exitGameScreen.SetActive(!exitGameScreen.activeSelf);

        if (exitGameScreen.activeSelf)
        {
            Time.timeScale = 0;
        }
        else if (!exitGameScreen.activeSelf && !GameOver)
        {
            Time.timeScale = 1;
        }
    }
}
