using UnityEngine;

// Ördek Avı: gidip gelen ördeklere top at. Alt sıra 1, orta sıra 2, üst sıra 3 puan.
public class DuckGame : MiniGame
{
    private Duck[] ducks;

    protected override void Awake()
    {
        base.Awake();

        ducks = GetComponentsInChildren<Duck>(true);
        foreach (Duck duck in ducks)
            duck.game = this;
    }

    public void OnDuckHit(Duck duck, Vector3 point)
    {
        SpawnEffect(point);
        AddScore(duck.points, point + Vector3.up * 0.3f);
    }
}
