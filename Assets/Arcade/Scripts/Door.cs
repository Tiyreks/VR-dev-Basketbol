using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Menteşeli kapı. Kapıyı tutma tuşuyla (grip) seçince açılır, tekrar seçince kapanır.
// Bu script menteşe objesindedir; kapı kanadı (XRSimpleInteractable) onun çocuğudur.
public class Door : MonoBehaviour
{
    public XRSimpleInteractable leaf;
    public float openAngle = -100f;
    public float speed = 160f;

    private bool open;
    private float angle;

    public bool IsOpen => open;

    private void Awake()
    {
        if (leaf == null)
            leaf = GetComponentInChildren<XRSimpleInteractable>();

        if (leaf != null)
            leaf.selectEntered.AddListener(OnSelected);
    }

    private void OnDestroy()
    {
        if (leaf != null)
            leaf.selectEntered.RemoveListener(OnSelected);
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        Toggle();
    }

    public void Toggle()
    {
        open = !open;
        ArcadeSound.Play(ArcadeSound.Kind.Bounce, transform.position + Vector3.up, 0.7f);
    }

    private void Update()
    {
        angle = Mathf.MoveTowards(angle, open ? openAngle : 0f, speed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
}
