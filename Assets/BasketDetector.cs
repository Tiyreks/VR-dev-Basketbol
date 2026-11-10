using UnityEngine;

using TMPro;


public class BasketDetector : MonoBehaviour

{

public TextMeshProUGUI scoreText;

[SerializeField] private ParticleSystem scoreEffectPrefab;

// Called on every basket (used for the hoop lights).
public UnityEngine.Events.UnityEvent onScore;

private int score = 0;

public int Score => score;


private void OnTriggerEnter(Collider other)

{
Rigidbody rb = other.attachedRigidbody;

if (!other.CompareTag("Basketball") || rb == null || rb.linearVelocity.y >= 0)
{
return;
}

score++;

ArcadeSound.Play(ArcadeSound.Kind.Swish, transform.position, 0.9f);
ArcadeSound.Play(ArcadeSound.Kind.Score, transform.position, 0.6f);

onScore?.Invoke();

if (scoreText != null)
{
scoreText.text = "Skor: " + score;
}

if (scoreEffectPrefab != null)
{
ParticleSystem effect = Instantiate(scoreEffectPrefab, transform.position, scoreEffectPrefab.transform.rotation);
effect.Play();
Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
}

Debug.Log("BASKET! +1 PUAN");

}


public void ResetScore()

{
score = 0;

if (scoreText != null)
{
scoreText.text = "Skor: " + score;
}

}

}