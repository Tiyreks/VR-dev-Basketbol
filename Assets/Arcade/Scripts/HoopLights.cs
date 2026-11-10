using UnityEngine;

// Bulbs around the basketball hoop. A slow chase while idle, and a short bright flash on every basket.
public class HoopLights : MonoBehaviour
{
    public Renderer[] bulbs;
    public Light flashLight;

    public Color bulbColor = new Color(1f, 0.55f, 0.1f);
    public float idleBrightness = 0.6f;
    public float chaseBrightness = 2.5f;
    public float chaseSpeed = 6f;
    public float flashBrightness = 5f;
    public float flashLightIntensity = 4f;
    public float flashDuration = 0.7f;

    private MaterialPropertyBlock block;
    private float flashTimer;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        block = new MaterialPropertyBlock();

        if (flashLight != null)
            flashLight.intensity = 0f;
    }

    // Hooked to BasketDetector.onScore.
    public void Flash()
    {
        flashTimer = flashDuration;
    }

    private void Update()
    {
        flashTimer = Mathf.Max(0f, flashTimer - Time.deltaTime);
        float flash = flashTimer / flashDuration;

        float chasePosition = Time.time * chaseSpeed;

        for (int i = 0; i < bulbs.Length; i++)
        {
            if (bulbs[i] == null)
                continue;

            // Distance (around the ring) from the moving "head" of the chase.
            float distance = Mathf.Repeat(chasePosition - i, bulbs.Length);
            float chase = Mathf.Max(0f, 1f - distance / 3f);
            float brightness = Mathf.Lerp(idleBrightness + chase * (chaseBrightness - idleBrightness), flashBrightness, flash);

            bulbs[i].GetPropertyBlock(block);
            block.SetColor(EmissionColor, bulbColor * brightness);
            bulbs[i].SetPropertyBlock(block);
        }

        if (flashLight != null)
            flashLight.intensity = flash * flashLightIntensity;
    }
}
