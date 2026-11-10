using UnityEngine;

// Kutu Devirme: kuledeki kutulara top at. Yere düşen her kutu 1 puan.
// Hepsi düşerse +3 bonus. Kulede yerinde duran kutu kalmayınca kule yeniden dizilir.
public class CanGame : MiniGame
{
    // Kutunun merkezi bu yüksekliğin altına inerse "yere düştü" sayılır.
    public float groundY = 0.1f;
    public int allDownBonus = 3;
    public Transform towerTop;

    private Can[] cans;
    private float settledTime;
    private float rebuildTimer = -1f;

    protected override void Awake()
    {
        base.Awake();
        cans = GetComponentsInChildren<Can>(true);
    }

    protected override void Update()
    {
        base.Update();

        if (cans == null || cans.Length == 0)
            return;

        if (rebuildTimer > 0f)
        {
            rebuildTimer -= Time.deltaTime;
            if (rebuildTimer <= 0f)
                RebuildTower();
            return;
        }

        int down = 0;
        int standing = 0;
        bool allResting = true;

        foreach (Can can in cans)
        {
            if (!can.counted && can.transform.position.y < groundY)
            {
                can.counted = true;
                SpawnEffect(can.transform.position);
                AddScore(1, can.transform.position + Vector3.up * 0.4f);
            }

            if (can.counted)
                down++;
            else if (!can.IsDisplaced)
                standing++;

            if (!can.IsResting)
                allResting = false;
        }

        if (down == cans.Length)
        {
            if (IsRunning)
            {
                Vector3 top = towerTop != null ? towerTop.position : transform.position + Vector3.up;
                AddScore(allDownBonus, top + Vector3.up * 0.3f);
                ShowInfo("HEPSİ DÜŞTÜ! +" + allDownBonus, 2f);
            }

            rebuildTimer = 1.5f;
            return;
        }

        // Kulede yerinde kutu kalmadıysa ve her şey durduysa (bazıları masada kaldı) yeniden diz.
        settledTime = standing == 0 && allResting ? settledTime + Time.deltaTime : 0f;
        if (settledTime > 2f)
            rebuildTimer = 0.01f;
    }

    private void RebuildTower()
    {
        rebuildTimer = -1f;
        settledTime = 0f;

        foreach (Can can in cans)
            can.ResetCan();

        ArcadeSound.Play(ArcadeSound.Kind.Ticket, transform.position, 0.5f);
    }

    protected override void OnGameStarted()
    {
        RebuildTower();
    }
}
