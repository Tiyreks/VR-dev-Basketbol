using UnityEngine;
using TMPro;

// Ortak oyun mantığı: süre, skor, rekor ve oyunun arkasındaki dünya panelini (world-space UI) yönetir.
// Buton yok: ilk sayıyla 60 saniyelik tur kendiliğinden başlar, süre bitince skor gösterilir,
// bir sonraki sayı yeni turu başlatır. Basketbol, Ördek Avı ve Kutu Devirme bu sınıftan türer.
public abstract class MiniGame : MonoBehaviour
{
    [Header("Oyun")]
    public string gameTitle = "OYUN";
    public float duration = 60f;
    [TextArea] public string howToPlay = "";
    public int tickSeconds = 5;
    // Tur bittikten sonra yeni turun başlayabilmesi için beklenen süre.
    public float restartDelay = 3f;

    [Header("Dünya Paneli")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI bestText;
    public TextMeshProUGUI infoText;

    [Header("Efekt")]
    public ParticleSystem hitEffect;
    public Color popupColor = Color.yellow;

    public bool IsRunning { get; private set; }
    public int Score { get; private set; }

    private float timeLeft;
    private int lastTick = -1;
    private int best;
    private float infoTimer;
    private float sinceEnd = 999f;

    private string BestKey => "Rekor_" + gameTitle;
    private string IdleInfo => howToPlay + "\n<size=80%>İlk sayıyla süre başlar!</size>";

    protected virtual void Awake()
    {
        best = PlayerPrefs.GetInt(BestKey, 0);
        timeLeft = duration;
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

        ShowInfo(IdleInfo, 0f);
        RefreshUI();
    }

    protected virtual void Update()
    {
        if (infoTimer > 0f)
        {
            infoTimer -= Time.deltaTime;
            if (infoTimer <= 0f)
                ShowInfo(IsRunning ? howToPlay : IdleInfo, 0f);
        }

        if (!IsRunning)
        {
            sinceEnd += Time.deltaTime;
            return;
        }

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

    public void StartGame()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        Score = 0;
        timeLeft = duration;
        lastTick = -1;

        ArcadeSound.Play(ArcadeSound.Kind.Start, transform.position, 0.8f);
        ShowInfo("BAŞLADI! " + Mathf.RoundToInt(duration) + " saniye!", 2f);

        OnGameStarted();
        RefreshUI();
    }

    public void EndGame()
    {
        if (!IsRunning)
            return;

        IsRunning = false;
        sinceEnd = 0f;

        ArcadeSound.Play(ArcadeSound.Kind.Buzzer, transform.position, 0.9f);

        bool newRecord = Score > best;
        if (newRecord)
        {
            best = Score;
            PlayerPrefs.SetInt(BestKey, best);
            PlayerPrefs.Save();
            ArcadeSound.Play(ArcadeSound.Kind.End, transform.position, 0.8f);
        }

        ShowInfo("SÜRE BİTTİ!\nSkorun: " + Score + (newRecord ? "  YENİ REKOR!" : ""), 8f);

        OnGameEnded();
        RefreshUI();
    }

    // Puan otomatik sayılır. Tur açık değilse (ve bitişin hemen ardından değilse) yeni tur başlar.
    public void AddScore(int points, Vector3 position)
    {
        if (!IsRunning)
        {
            if (sinceEnd < restartDelay)
                return;
            StartGame();
        }

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
    }

    protected virtual void OnGameStarted() { }
    protected virtual void OnGameEnded() { }
}
