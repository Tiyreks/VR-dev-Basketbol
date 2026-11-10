using UnityEngine;
using TMPro;

// Vurulan yerde kısa süre görünen, yukarı süzülüp kaybolan "+1" yazısı (dünyada, HUD değil).
public class ScorePopup : MonoBehaviour
{
    private const float Life = 1.2f;

    private TextMeshPro text;
    private Color color;
    private float age;

    public static void Show(string message, Vector3 position, Color color, float size = 1f)
    {
        GameObject go = new GameObject("SkorYazisi");
        go.transform.position = position;

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = 4f * size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.color = color;
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = Color.black;

        ScorePopup popup = go.AddComponent<ScorePopup>();
        popup.text = tmp;
        popup.color = color;
    }

    private void Update()
    {
        age += Time.deltaTime;

        transform.position += Vector3.up * (0.6f * Time.deltaTime);

        Camera cam = Camera.main;
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

        float k = age / Life;
        text.color = new Color(color.r, color.g, color.b, 1f - k * k);
        transform.localScale = Vector3.one * (k < 0.15f ? Mathf.Lerp(0.3f, 1.2f, k / 0.15f) : 1f);

        if (age >= Life)
            Destroy(gameObject);
    }
}
