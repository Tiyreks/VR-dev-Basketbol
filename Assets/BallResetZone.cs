using UnityEngine;

public class BallResetZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Basketball"))
        {
            BallReset ball = other.GetComponent<BallReset>();

            if (ball != null)
            {
                ball.ResetBall();
            }
        }
    }
}
