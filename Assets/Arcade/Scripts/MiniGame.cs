using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Ortak oyun mantığı: süre, skor, rekor ve oyunun yanındaki dünya panelini (world-space UI) yönetir.
// Basketbol, Ördek Avı ve Kutu Devirme bu sınıftan türer.
public abstract class MiniGame : MonoBehaviour
{
    [Header("Oyun")]
    public string gameTitle = "OYUN";
    public float duration = 60f;
    [TextArea] public string howToPlay = "";
    public int tickSeconds = 5;

    [Header("Dünya Paneli")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI bestText;
    public TextMeshProUGUI infoText;
    public Button startButton;
    public TextMeshProUGUI startButtonLabel;
    public Image startButtonImage;

    [Header("Efekt")]
    public ParticleSystem hitEffect;
    public Color popupColor = Color.yellow;

    public bool IsRunning { get; private set; }
    public int Score { get; private set; }

    private float timeLeft;
    private int lastTick = -1;
    private int best;
    private float infoTimer;

    private static readonly Color StartColor = new Color(0.2f, 0.7f, 0.2f);
    private static readonly Color StopColor = new Color(0.8f, 0.2f, 0.15f);

    private string BestKey => "Rekor_" + gameTitle;

    protected virtual void Awake()
    {
        best = PlayerPrefs.GetInt(BestKey, 0);
        timeLeft = duration;

        if (startButton != null)
            startButton.onClick.AddListener(ToggleGame);
    }

    protected virtual void Start()
    {
        // World-space canvas'lar için olay kamerası.
        Camera cam = Camera.main;
        foreach (Canvas canvas in GetComponentsInChildren<Canvas>(true))
        {
            if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
                canvas.worldCamera = cam;
        }

        if (titleText != null)
            titleText.text = gameTitle;

        ShowInfo(howToPlay, 0f);
        RefreshUI();
    }

    protected virtual void Update()
    {
        if (infoTimer > 0f)
        {
            infoTimer -= Time.deltaTime;
            if (infoTimer <= 0f)
                ShowInfo(howToPlay, 0f);
        }

        if (!IsRunning)
            return;

        timeLeft -= Time.deltaTime;

        int second = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
        if (second <= tickSeconds && second > 0 && second != lastTick)
        {
            lastTick = second;
            ArcadeSound.Play(ArcadeSound.Kind.Tick, transform.position, 0.8f);
        }

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndGame();
            return;
        }

        RefreshUI();
    }

    // Paneldeki butona bağlı: oyun yoksa başlatır, varsa bitirir.
    public void ToggleGame()
    {
        if (IsRunning)
            EndGame();
        else
            StartGame();
    }

    public void StartGame()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        Score = 0;
        timeLeft = duration;
        lastTick = -1;

        ArcadeSound.Play(ArcadeSound.Kind.Start, transform.position, 0.8f);
        ShowInfo("BAŞLA!", 1.5f);

        OnGameStarted();
        RefreshUI();
    }

    public void EndGame()
    {
        if (!IsRunning)
            return;

        IsRunning = false;

        ArcadeSound.Play(ArcadeSound.Kind.Buzzer, transform.position, 0.9f);

        bool newRecord = Score > best;
        if (newRecord)
        {
            best = Score;
            PlayerPrefs.SetInt(BestKey, best);
            PlayerPrefs.Save();
            ArcadeSound.Play(ArcadeSound.Kind.End, transform.position, 0.8f);
        }

        ShowInfo("OYUN BİTTİ!\nSkorun: " + Score + (newRecord ? "\nYENİ REKOR!" : ""), 6f);

        OnGameEnded();
        RefreshUI();
    }

    // Puan sadece oyun açıkken sayılır. Vurulan yerde "+1" yazısı çıkar.
    public void AddScore(int points, Vector3 position)
    {
        if (!IsRunning)
            return;

        Score += points;
        ScorePopup.Show("+" + points, position, popupColor);
        ArcadeSound.Play(ArcadeSound.Kind.Score, position, 0.6f);
        RefreshUI();
    }

    public void SpawnEffect(Vector3 position)
    {
        if (hitEffect == null)
            return;

        ParticleSystem effect = Instantiate(hitEffect, position, hitEffect.transform.rotation);
        effect.Play();
        Destroy(effect.gameObject, 4f);
    }

    protected void ShowInfo(string message, float seconds)
    {
        if (infoText != null)
            infoText.text = message;

        infoTimer = seconds;
    }

    protected void RefreshUI()
    {
        if (scoreText != null)
            scoreText.text = "SKOR: " + Score;

        if (timeText != null)
            timeText.text = "SÜRE: " + Mathf.CeilToInt(Mathf.Max(0f, timeLeft));

        if (bestText != null)
            bestText.text = "REKOR: " + best;

        if (startButtonLabel != null)
            startButtonLabel.text = IsRunning ? "BİTİR" : "BAŞLAT";

        if (startButtonImage != null)
            startButtonImage.color = IsRunning ? StopColor : StartColor;
    }

    protected virtual void OnGameStarted() { }
    protected virtual void OnGameEnded() { }
}
