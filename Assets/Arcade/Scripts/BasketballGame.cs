using UnityEngine;

// Basketbol: potadaki BasketDetector her basket attığında oyun açıksa 1 puan verir.
public class BasketballGame : MiniGame
{
    public BasketDetector detector;

    protected override void Awake()
    {
        base.Awake();

        if (detector != null)
            detector.onScore.AddListener(OnBasket);
    }

    private void OnBasket()
    {
        AddScore(1, detector.transform.position + Vector3.up * 0.6f);
    }

    protected override void OnGameStarted()
    {
        if (detector != null)
            detector.ResetScore();
    }
}
