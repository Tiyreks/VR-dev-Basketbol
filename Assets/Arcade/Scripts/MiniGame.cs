using UnityEngine;
using TMPro;

// Ortak oyun mantığı: sadece skor. Süre, rekor ya da başlat butonu yok; her sayı otomatik eklenir.
// Skor, oyunun arkasındaki dünya panelinde (world-space UI) görünür.
// Basketbol, Ördek Avı ve Kutu Devirme bu sınıftan türer.
public abstract class MiniGame : MonoBehaviour
{
    [Header("Oyun")]
    public string gameTitle = "OYUN";

    [Header("Dünya Paneli")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;

    [Header("Efekt")]
    public ParticleSystem hitEffect;
    public Color popupColor = Color.yellow;

    public int Score { get; private set; }

    protected virtual void Awake() { }

    protected virtual void Start()
    {
        Camera cam = Camera.main;
        foreach (Canvas canvas in GetComponentsInChildren<Canvas>(true))
        {
            if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
                canvas.worldCamera = cam;
        }

        if (titleText != null)
            titleText.text = gameTitle;

        RefreshUI();
    }

    protected virtual void Update() { }

    // Vurulan yerde "+1" yazısı çıkar ve skor artar.
    public void AddScore(int points, Vector3 position)
    {
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

    protected void RefreshUI()
    {
        if (scoreText != null)
            scoreText.text = "SKOR: " + Score;
    }
}
