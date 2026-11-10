using UnityEngine;

// Süs atari makinesinin ekranı: renkleri yavaşça değişir, küçük bir "piksel" ekranda seker,
// "JETON AT" yazısı yanıp söner. Oynanmaz, sadece dekor.
public class ArcadeScreen : MonoBehaviour
{
    public Renderer screen;
    public Transform pixel;
    public GameObject blinkText;

    // Pikselin ekran içinde gezdiği alan (ekranın yerel düzleminde, yarı genişlik/yükseklik).
    public Vector2 bounds = new Vector2(0.22f, 0.17f);
    public Vector2 velocity = new Vector2(0.17f, 0.11f);

    private static readonly Color[] Palette =
    {
        new Color(0.05f, 0.05f, 0.35f), new Color(0.3f, 0.02f, 0.3f), new Color(0.02f, 0.25f, 0.2f)
    };

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    private MaterialPropertyBlock block;
    private Vector2 position;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (screen != null)
        {
            float t = Time.time * 0.3f;
            int i = Mathf.FloorToInt(t) % Palette.Length;
            Color c = Color.Lerp(Palette[i], Palette[(i + 1) % Palette.Length], t - Mathf.Floor(t));
            screen.GetPropertyBlock(block);
            block.SetColor(BaseColor, c);
            screen.SetPropertyBlock(block);
        }

        if (pixel != null)
        {
            position += velocity * Time.deltaTime;
            if (Mathf.Abs(position.x) > bounds.x) { velocity.x = -velocity.x; position.x = Mathf.Clamp(position.x, -bounds.x, bounds.x); }
            if (Mathf.Abs(position.y) > bounds.y) { velocity.y = -velocity.y; position.y = Mathf.Clamp(position.y, -bounds.y, bounds.y); }
            pixel.localPosition = new Vector3(position.x, position.y, pixel.localPosition.z);
        }

        if (blinkText != null)
            blinkText.SetActive(Mathf.Repeat(Time.time, 1.2f) < 0.8f);
    }
}
