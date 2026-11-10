using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public GameObject gameOverText;
    public GameObject restartButton;
    public GameObject startButton;
    public GameObject endGameButton;

    public float timeRemaining = 60f;

    public BasketDetector scoreManager;

    // Countdown ticks play during the last few seconds.
    public int tickSeconds = 5;

    private bool gameStarted = false;
    private int lastTickSecond = -1;

    // GameManager sits far from the court, so sounds play at the hoop instead.
    private Vector3 SoundPosition => scoreManager != null ? scoreManager.transform.position : transform.position;

    void Start()
    {
        timeRemaining = 60f;

        timerText.text = "SÜRE: 60";

        gameOverText.SetActive(false);
        restartButton.SetActive(false);
        endGameButton.SetActive(false);
    }

    void Update()
    {
        if (gameStarted && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining < 0)
                timeRemaining = 0;

            int second = Mathf.CeilToInt(timeRemaining);
            timerText.text = "SÜRE: " + second;

            if (second <= tickSeconds && second > 0 && second != lastTickSecond)
            {
                lastTickSecond = second;
                ArcadeSound.Play(ArcadeSound.Kind.Tick, SoundPosition, 0.8f);
            }
        }
        else if (gameStarted && timeRemaining <= 0)
        {
            EndGame();
        }
    }

    private void BeginRound()
    {
        lastTickSecond = -1;
        ArcadeSound.Play(ArcadeSound.Kind.Start, SoundPosition, 0.8f);
    }

    public void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        timeRemaining = 60f;

        timerText.text = "SÜRE: 60";

        startButton.SetActive(false);
        endGameButton.SetActive(true);

        scoreManager.ResetScore();
        BeginRound();
    }

    public void EndGame()
    {
        if (!gameStarted)
            return;

        gameStarted = false;

        ArcadeSound.Play(ArcadeSound.Kind.Buzzer, SoundPosition, 0.9f);

        gameOverText.SetActive(true);
        restartButton.SetActive(true);
        endGameButton.SetActive(false);
    }

    public void RestartGame()
    {
        if (gameStarted)
            return;

        timeRemaining = 60f;

        gameStarted = true;

        timerText.text = "SÜRE: 60";

        gameOverText.SetActive(false);
        restartButton.SetActive(false);
        endGameButton.SetActive(true);

        scoreManager.ResetScore();
        BeginRound();
    }
}
