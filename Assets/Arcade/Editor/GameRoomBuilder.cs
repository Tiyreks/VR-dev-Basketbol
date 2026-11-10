using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Menü: Arcade > Oyun Odasını Kur
// Oyun odasını, 3 oyun standını (Basketbol, Ördek Avı, Kutu Devirme), dünya panellerini ve
// parçacık efektlerini SampleScene içine basit primitiflerle kurar. Tekrar çalıştırmak güvenlidir:
// eski "OYUN_ODASI" silinip baştan kurulur.
[InitializeOnLoad]
public static class GameRoomBuilder
{
    const string RootName = "OYUN_ODASI";
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    const string MatFolder = "Assets/Arcade/Materials/Oda";
    const string PrefabFolder = "Assets/Arcade/Prefabs";
    const float Y0 = -0.15f; // zemin yüksekliği

    static readonly Color Wood = new Color(0.55f, 0.35f, 0.18f);
    static readonly Color Red = new Color(0.85f, 0.15f, 0.12f);
    static readonly Color White = new Color(0.95f, 0.95f, 0.92f);
    static readonly Color Blue = new Color(0.15f, 0.35f, 0.75f);
    static readonly Color DarkBlue = new Color(0.1f, 0.18f, 0.4f);
    static readonly Color LightBlue = new Color(0.4f, 0.7f, 0.95f);
    static readonly Color Yellow = new Color(1f, 0.85f, 0.1f);
    static readonly Color Orange = new Color(1f, 0.5f, 0.05f);
    static readonly Color Green = new Color(0.2f, 0.55f, 0.25f);

    static string RequestFile => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "OyunOdasi.request");

    static GameRoomBuilder()
    {
        EditorApplication.delayCall += CheckRequest;
    }

    // Proje klasörüne "OyunOdasi.request" dosyası konursa derlemeden sonra oda otomatik kurulur.
    static void CheckRequest()
    {
        if (!File.Exists(RequestFile))
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += CheckRequest;
            return;
        }

        File.Delete(RequestFile);
        Build();
    }

    [MenuItem("Arcade/Oyun Odasını Kur")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[OyunOdasi] Play modundayken kurulamaz.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            if (scene.isDirty)
            {
                Debug.LogWarning("[OyunOdasi] Önce açık sahneyi kaydedin, sonra tekrar deneyin.");
                return;
            }
            scene = EditorSceneManager.OpenScene(ScenePath);
        }

        EnsureFolder("Assets/Arcade", "Prefabs");
        EnsureFolder("Assets/Arcade/Materials", "Oda");

        GameObject old = GameObject.Find(RootName);
        if (old != null)
            Object.DestroyImmediate(old);

        // Eski, bozuk referanslı zamanlayıcı (UI'ı silinmişti). Yerine her standın kendi paneli var.
        GameObject oldManager = GameObject.Find("GameManager");
        if (oldManager != null && oldManager.GetComponent<GameTimer>() != null)
            Object.DestroyImmediate(oldManager);

        Transform root = new GameObject(RootName).transform;

        ParticleSystem basketFx = MakeEffect("BasketEfekti",
            new[] { Red, Yellow, Blue, Green, Orange, new Color(0.8f, 0.2f, 0.9f) }, 90, 4.5f, 0.06f, 0.7f, true);
        ParticleSystem duckFx = MakeEffect("OrdekEfekti",
            new[] { Color.white, Yellow, new Color(1f, 0.95f, 0.6f) }, 35, 2f, 0.05f, 0.25f, false);
        ParticleSystem canFx = MakeEffect("KutuEfekti",
            new[] { Yellow, Orange, new Color(0.75f, 0.75f, 0.8f) }, 25, 2.5f, 0.04f, 0.5f, false);

        BuildRoom(root);
        BuildBasketball(root, basketFx);
        BuildDuckGame(root, duckFx);
        BuildCanGame(root, canFx);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[OyunOdasi] Oyun odası kuruldu ve sahne kaydedildi.");
    }

    // ---------------------------------------------------------------- ODA

    static void BuildRoom(Transform root)
    {
        Transform room = Group("Oda", root, Vector3.zero);

        // Var olan zemini odayı kaplayacak kadar büyüt (14 x 14 m).
        GameObject floor = GameObject.Find("Zemin");
        if (floor != null)
        {
            floor.transform.position = new Vector3(0f, Y0, 1f);
            floor.transform.localScale = new Vector3(1.4f, floor.transform.localScale.y, 1.4f);
        }

        Material wall = Mat("Duvar", new Color(0.93f, 0.87f, 0.72f));
        Material wall2 = Mat("Duvar2", new Color(0.75f, 0.85f, 0.95f));
        Material board = Mat("Supurgelik", new Color(0.3f, 0.2f, 0.12f));

        const float h = 3.2f;
        float cy = Y0 + h / 2f;
        Box("Duvar_Kuzey", room, new Vector3(0f, cy, 8.1f), new Vector3(14.4f, h, 0.2f), wall);
        Box("Duvar_Guney", room, new Vector3(0f, cy, -6.1f), new Vector3(14.4f, h, 0.2f), wall);
        Box("Duvar_Bati", room, new Vector3(-7.1f, cy, 1f), new Vector3(0.2f, h, 14.4f), wall2);
        Box("Duvar_Dogu", room, new Vector3(7.1f, cy, 1f), new Vector3(0.2f, h, 14.4f), wall2);

        // Süpürgelik ve duvarın ortasında renkli bir şerit.
        float by = Y0 + 0.08f;
        Box("Supurgelik_K", room, new Vector3(0f, by, 7.98f), new Vector3(14f, 0.16f, 0.04f), board, false);
        Box("Supurgelik_G", room, new Vector3(0f, by, -5.98f), new Vector3(14f, 0.16f, 0.04f), board, false);
        Box("Supurgelik_B", room, new Vector3(-6.98f, by, 1f), new Vector3(0.04f, 0.16f, 14f), board, false);
        Box("Supurgelik_D", room, new Vector3(6.98f, by, 1f), new Vector3(0.04f, 0.16f, 14f), board, false);

        Material stripe = Mat("Serit", Red);
        Box("Serit_G", room, new Vector3(0f, Y0 + 1.2f, -5.98f), new Vector3(14f, 0.12f, 0.03f), stripe, false);
        Box("Serit_B", room, new Vector3(-6.98f, Y0 + 1.2f, 1f), new Vector3(0.03f, 0.12f, 14f), stripe, false);

        // Güney duvarında el yapımı tabela.
        Box("Tabela", room, new Vector3(0f, Y0 + 2.45f, -5.97f), new Vector3(4.2f, 0.9f, 0.04f), Mat("Tabela", Yellow), false);
        Sign("Tabela_Yazi", room, "OYUN ODASI", new Vector3(0f, Y0 + 2.45f, -5.94f), 180f, 7f, Red);

        // Kapı (süs).
        Box("Kapi", room, new Vector3(4.5f, Y0 + 1.05f, -5.98f), new Vector3(1.1f, 2.1f, 0.05f), Mat("Kapi", Wood), false);
        Box("Kapi_Tokmak", room, new Vector3(4.15f, Y0 + 1.0f, -5.94f), new Vector3(0.06f, 0.06f, 0.06f), Mat("Metal", new Color(0.8f, 0.7f, 0.2f)), false);
    }

    // ---------------------------------------------------------------- BASKETBOL

    static void BuildBasketball(Transform root, ParticleSystem fx)
    {
        Transform booth = Group("BASKETBOL", root, new Vector3(-4f, 1f, 0.5f));
        BasketballGame game = booth.gameObject.AddComponent<BasketballGame>();
        game.gameTitle = "BASKETBOL";
        game.howToPlay = "Topu al ve potaya at!\nHer basket 1 puan.";
        game.popupColor = new Color(1f, 0.6f, 0.1f);

        BasketDetector detector = Object.FindFirstObjectByType<BasketDetector>();
        game.detector = detector;

        // Basket parçacıkları: yeni konfeti + halka efekti.
        if (detector != null)
        {
            SerializedObject so = new SerializedObject(detector);
            so.FindProperty("scoreEffectPrefab").objectReferenceValue = fx;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        Material wood = Mat("Tahta", Wood);
        Material white = Mat("Beyaz", White);

        // Pota panosu ve ampuller.
        Box("Pano", booth, new Vector3(-5.58f, 3.05f, -0.45f), new Vector3(0.05f, 1.1f, 1.8f), white);
        Box("Pano_Kare", booth, new Vector3(-5.55f, 2.95f, -0.45f), new Vector3(0.01f, 0.45f, 0.6f), Mat("Kirmizi", Red), false);

        HoopLights lights = Object.FindFirstObjectByType<HoopLights>();
        if (lights != null)
        {
            Material bulb = AssetDatabase.LoadAssetAtPath<Material>("Assets/Arcade/Materials/Hoop_Bulb.mat");
            Transform bulbs = Group("Pota_Ampulleri", booth, Vector3.zero);
            Renderer[] list = new Renderer[16];
            for (int i = 0; i < list.Length; i++)
            {
                // Panonun kenarı boyunca dikdörtgen bir halka.
                float t = i / (float)list.Length * 4f;
                float y, z;
                if (t < 1f) { y = 2.55f; z = Mathf.Lerp(-1.3f, 0.4f, t); }
                else if (t < 2f) { y = Mathf.Lerp(2.55f, 3.55f, t - 1f); z = 0.4f; }
                else if (t < 3f) { y = 3.55f; z = Mathf.Lerp(0.4f, -1.3f, t - 2f); }
                else { y = Mathf.Lerp(3.55f, 2.55f, t - 3f); z = -1.3f; }

                GameObject b = Prim(PrimitiveType.Sphere, "Ampul_" + i, bulbs, new Vector3(-5.54f, y, z), Vector3.one * 0.07f, bulb, false);
                list[i] = b.GetComponent<Renderer>();
            }
            lights.bulbs = list;
            EditorUtility.SetDirty(lights);
        }

        // Top rafı.
        Vector3 rack = new Vector3(-2.9f, 0f, -1.6f);
        float top = Table("Raf", booth, rack, new Vector2(0.5f, 1.0f), 0.8f, wood, white);

        GameObject ball = GameObject.Find("TOPP");
        if (ball != null)
        {
            ball.transform.position = new Vector3(rack.x, top + 0.125f, rack.z - 0.3f);
            AutoReturnBall ar = ball.GetComponent<AutoReturnBall>();
            if (ar == null)
                ar = ball.AddComponent<AutoReturnBall>();
            ar.playBounceSound = false; // BallReset zaten zıplama sesi çalıyor.

            for (int i = 1; i <= 2; i++)
            {
                GameObject copy = Object.Instantiate(ball, booth);
                copy.name = "TOPP (" + (i + 1) + ")";
                copy.transform.position = new Vector3(rack.x, top + 0.125f, rack.z - 0.3f + i * 0.3f);
            }
        }

        Line("Atis_Cizgisi", booth, new Vector3(-2.3f, Y0 + 0.005f, -0.45f), new Vector3(0.06f, 0.01f, 2.4f));
        Lamp("Lamba", booth, new Vector3(-3.8f, 2.6f, 0f));

        BuildPanel(game, booth, new Vector3(-3.6f, Y0 + 1.65f, 1.4f), -90f, new Color(0.45f, 0.25f, 0.1f), fx);
    }

    // ---------------------------------------------------------------- ÖRDEK AVI

    static void BuildDuckGame(Transform root, ParticleSystem fx)
    {
        Transform booth = Group("ORDEK_AVI", root, new Vector3(1f, 1f, 6f));
        DuckGame game = booth.gameObject.AddComponent<DuckGame>();
        game.gameTitle = "ÖRDEK AVI";
        game.howToPlay = "Gidip gelen ördeklere top at!\nAlt sıra 1, orta 2, üst sıra 3 puan.";
        game.popupColor = Yellow;

        Material wood = Mat("Tahta", Wood);
        Material white = Mat("Beyaz", White);
        Material red = Mat("Kirmizi", Red);
        Material water = Mat("Su", LightBlue);

        Box("Arka_Pano", booth, new Vector3(1f, Y0 + 1.6f, 7.75f), new Vector3(7.6f, 3.2f, 0.1f), Mat("Lacivert", DarkBlue));
        Box("Yan_Pano_Sol", booth, new Vector3(-2.85f, Y0 + 1.6f, 6.5f), new Vector3(0.1f, 3.2f, 2.6f), Mat("Lacivert", DarkBlue));
        Box("Yan_Pano_Sag", booth, new Vector3(4.85f, Y0 + 1.6f, 6.5f), new Vector3(0.1f, 3.2f, 2.6f), Mat("Lacivert", DarkBlue));

        Transform ducks = Group("Ordekler", booth, new Vector3(1f, 0f, 7.2f));
        float[] speeds = { 0.9f, 1.3f, 1.8f };
        const float track = 6.4f;
        const float gap = 1.4f;

        for (int row = 0; row < 3; row++)
        {
            float railY = Y0 + 0.75f + row * 0.6f;

            Box("Ray_" + row, booth, new Vector3(1f, railY - 0.04f, 7.2f), new Vector3(7.4f, 0.08f, 0.12f), wood);
            Box("Dalga_" + row, booth, new Vector3(1f, railY, 7.05f), new Vector3(7.4f, 0.16f, 0.03f), water);
            for (int w = 0; w < 9; w++)
                Prim(PrimitiveType.Sphere, "Dalga_Tepe_" + row + "_" + w, booth,
                    new Vector3(-2.6f + w * 0.9f, railY + 0.08f, 7.05f), new Vector3(0.3f, 0.12f, 0.03f), water, false);

            for (int k = 0; k < 3; k++)
            {
                float minX = -track / 2f + k * gap;
                float maxX = minX + track - 2f * gap;
                BuildDuck(ducks, "Ordek_" + row + "_" + k, new Vector3(1f + minX, railY, 7.2f),
                    minX, maxX, speeds[row], row * 1.8f, row + 1);
            }
        }

        // Tezgah ve toplar.
        Box("Tezgah", booth, new Vector3(1f, Y0 + 0.5f, 4.6f), new Vector3(4.4f, 1.0f, 0.5f), red);
        float top = Y0 + 1.02f;
        Box("Tezgah_Ust", booth, new Vector3(1f, top - 0.01f, 4.6f), new Vector3(4.5f, 0.02f, 0.56f), white);
        Lips(booth, new Vector3(1f, top, 4.6f), new Vector2(4.5f, 0.56f), wood);

        Color[] ballColors = { Red, Blue, Green, Orange, new Color(0.8f, 0.2f, 0.9f) };
        for (int i = 0; i < 5; i++)
            Ball(booth, "Top_" + i, new Vector3(1f - 1.6f + i * 0.8f, top + 0.065f, 4.6f), 0.12f, Mat("Top_" + i, ballColors[i]));

        Awning(booth, new Vector3(1f, 0f, 4.6f), false, 4.8f, "ÖRDEK AVI");
        Line("Atis_Cizgisi", booth, new Vector3(1f, Y0 + 0.005f, 3.9f), new Vector3(4.4f, 0.01f, 0.06f));
        Lamp("Lamba", booth, new Vector3(1f, 2.6f, 6.2f));

        BuildPanel(game, booth, new Vector3(-2.05f, Y0 + 1.65f, 4.6f), 0f, new Color(0.1f, 0.3f, 0.55f), fx);
    }

    static void BuildDuck(Transform parent, string name, Vector3 pos, float minX, float maxX, float speed, float phase, int points)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        BoxCollider box = go.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.32f, 0f);
        box.size = new Vector3(0.44f, 0.38f, 0.3f);

        Transform pivot = Group("Pivot", go.transform, pos);

        Material yellow = Mat("Ordek_Sari", Yellow);
        Material wing = Mat("Ordek_Kanat", new Color(0.9f, 0.7f, 0.05f));
        Material orange = Mat("Ordek_Gaga", Orange);
        Material black = Mat("Siyah", Color.black);

        Prim(PrimitiveType.Cylinder, "Cubuk", pivot, pos + new Vector3(0f, 0.08f, 0f), new Vector3(0.03f, 0.08f, 0.03f), Mat("Tahta", Wood), false);

        // Hedef tahtası (oyuncuya bakar, dönmez).
        Vector3 disc = new Vector3(0f, 0.3f, -0.14f);
        Prim(PrimitiveType.Cylinder, "Hedef_1", pivot, pos + disc, new Vector3(0.15f, 0.004f, 0.15f), Mat("Kirmizi", Red), false, new Vector3(90f, 0f, 0f));
        Prim(PrimitiveType.Cylinder, "Hedef_2", pivot, pos + disc + new Vector3(0f, 0f, -0.004f), new Vector3(0.09f, 0.004f, 0.09f), Mat("Beyaz", White), false, new Vector3(90f, 0f, 0f));
        Prim(PrimitiveType.Cylinder, "Hedef_3", pivot, pos + disc + new Vector3(0f, 0f, -0.008f), new Vector3(0.035f, 0.004f, 0.035f), Mat("Kirmizi", Red), false, new Vector3(90f, 0f, 0f));

        // Ördek gövdesi (gagası +Z'ye bakar; Duck scripti yürüme yönüne çevirir).
        Transform model = Group("Model", pivot, pos);
        Prim(PrimitiveType.Sphere, "Govde", model, pos + new Vector3(0f, 0.28f, 0f), new Vector3(0.22f, 0.22f, 0.34f), yellow, false);
        Prim(PrimitiveType.Sphere, "Kafa", model, pos + new Vector3(0f, 0.45f, 0.12f), Vector3.one * 0.17f, yellow, false);
        Prim(PrimitiveType.Cube, "Gaga", model, pos + new Vector3(0f, 0.44f, 0.23f), new Vector3(0.08f, 0.035f, 0.09f), orange, false);
        Prim(PrimitiveType.Sphere, "Goz_L", model, pos + new Vector3(-0.07f, 0.48f, 0.17f), Vector3.one * 0.035f, black, false);
        Prim(PrimitiveType.Sphere, "Goz_R", model, pos + new Vector3(0.07f, 0.48f, 0.17f), Vector3.one * 0.035f, black, false);
        Prim(PrimitiveType.Sphere, "Kuyruk", model, pos + new Vector3(0f, 0.35f, -0.17f), new Vector3(0.08f, 0.1f, 0.1f), yellow, false);
        Prim(PrimitiveType.Sphere, "Kanat_L", model, pos + new Vector3(-0.1f, 0.3f, -0.02f), new Vector3(0.04f, 0.12f, 0.2f), wing, false);
        Prim(PrimitiveType.Sphere, "Kanat_R", model, pos + new Vector3(0.1f, 0.3f, -0.02f), new Vector3(0.04f, 0.12f, 0.2f), wing, false);

        Duck duck = go.AddComponent<Duck>();
        duck.minX = minX;
        duck.maxX = maxX;
        duck.speed = speed;
        duck.phase = phase;
        duck.points = points;
        duck.pivot = pivot;
        duck.model = model;
    }

    // ---------------------------------------------------------------- KUTU DEVİRME

    static void BuildCanGame(Transform root, ParticleSystem fx)
    {
        Transform booth = Group("KUTU_DEVIRME", root, new Vector3(4.5f, 1f, 1f));
        CanGame game = booth.gameObject.AddComponent<CanGame>();
        game.gameTitle = "KUTU DEVİRME";
        game.howToPlay = "Kutuları yere düşür!\nHer kutu 1 puan, hepsi düşerse +3.";
        game.popupColor = new Color(0.3f, 1f, 0.4f);
        game.groundY = Y0 + 0.25f;

        Material wood = Mat("Tahta", Wood);
        Material white = Mat("Beyaz", White);

        // Kutu masası.
        Vector3 tablePos = new Vector3(5.9f, 0f, 1f);
        float tableTop = Table("Kutu_Masasi", booth, tablePos, new Vector2(0.8f, 1.3f), 0.9f, wood, null);
        Box("Masa_Ortusu", booth, new Vector3(5.49f, Y0 + 0.6f, 1f), new Vector3(0.02f, 0.6f, 1.3f), Mat("Kirmizi", Red));
        Box("Arka_Perde", booth, new Vector3(6.75f, Y0 + 1.5f, 1f), new Vector3(0.1f, 3f, 3.4f), Mat("Yesil", Green));

        // 3-2-1 kutu kulesi.
        Transform cans = Group("Kutular", booth, Vector3.zero);
        Material[] canMats = { Mat("Kutu_Kirmizi", Red), Mat("Kutu_Mavi", Blue), Mat("Kutu_Yesil", Green) };
        Material band = Mat("Kutu_Etiket", White);
        const float canH = 0.2f, canD = 0.14f, spacing = 0.15f;
        int index = 0;
        for (int row = 0; row < 3; row++)
        {
            int count = 3 - row;
            for (int j = 0; j < count; j++)
            {
                Vector3 p = new Vector3(tablePos.x, tableTop + canH / 2f + row * canH + 0.001f, tablePos.z + (j - (count - 1) / 2f) * spacing);
                BuildCan(cans, "Kutu_" + index, p, canD, canH, canMats[index % canMats.Length], band);
                index++;
            }
        }
        Transform towerTop = Group("Kule_Tepesi", booth, new Vector3(tablePos.x, tableTop + 3 * canH, tablePos.z));
        game.towerTop = towerTop;

        // Atış tezgahı ve toplar.
        Box("Tezgah", booth, new Vector3(3f, Y0 + 0.5f, 1f), new Vector3(0.5f, 1.0f, 2.6f), Mat("Mavi", Blue));
        float top = Y0 + 1.02f;
        Box("Tezgah_Ust", booth, new Vector3(3f, top - 0.01f, 1f), new Vector3(0.56f, 0.02f, 2.7f), white);
        Lips(booth, new Vector3(3f, top, 1f), new Vector2(0.56f, 2.7f), wood);

        Color[] ballColors = { Yellow, Orange, Red, Green, LightBlue };
        for (int i = 0; i < 5; i++)
            Ball(booth, "Top_" + i, new Vector3(3f, top + 0.065f, 1f - 0.9f + i * 0.45f), 0.12f, Mat("KTop_" + i, ballColors[i]));

        Awning(booth, new Vector3(3f, 0f, 1f), true, 2.8f, "KUTU DEVİRME");
        Line("Atis_Cizgisi", booth, new Vector3(2.4f, Y0 + 0.005f, 1f), new Vector3(0.06f, 0.01f, 2.8f));
        Lamp("Lamba", booth, new Vector3(4.6f, 2.6f, 1f));

        BuildPanel(game, booth, new Vector3(3f, Y0 + 1.65f, -1.25f), 90f, new Color(0.15f, 0.4f, 0.2f), fx);
    }

    static void BuildCan(Transform parent, string name, Vector3 pos, float diameter, float height, Material mat, Material bandMat)
    {
        GameObject can = Prim(PrimitiveType.Cylinder, name, parent, pos, new Vector3(diameter, height / 2f, diameter), mat, false);
        can.AddComponent<BoxCollider>(); // kutu şeklinde çarpışma: üst üste daha sağlam durur

        Rigidbody rb = can.AddComponent<Rigidbody>();
        rb.mass = 0.25f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        GameObject label = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        label.name = "Etiket";
        Object.DestroyImmediate(label.GetComponent<Collider>());
        label.transform.SetParent(can.transform, false);
        label.transform.localScale = new Vector3(1.04f, 0.35f, 1.04f);
        label.GetComponent<Renderer>().sharedMaterial = bandMat;

        can.AddComponent<Can>();
    }

    // ---------------------------------------------------------------- DÜNYA PANELİ (UI)

    static void BuildPanel(MiniGame game, Transform parent, Vector3 pos, float yaw, Color bg, ParticleSystem fx)
    {
        game.hitEffect = fx;

        GameObject go = new GameObject("Panel", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        go.AddComponent<TrackedDeviceGraphicRaycaster>();

        RectTransform rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(500f, 460f);
        rt.position = pos;
        rt.rotation = Quaternion.Euler(0f, yaw, 0f);
        rt.localScale = Vector3.one * 0.002f;

        Image back = UIImage("Arkaplan", rt, Vector2.zero, new Vector2(500f, 460f), bg);
        Outline outline = back.gameObject.AddComponent<Outline>();
        outline.effectColor = Yellow;
        outline.effectDistance = new Vector2(8f, -8f);

        game.titleText = UIText("Baslik", rt, new Vector2(0f, 180f), new Vector2(480f, 70f), 50f, Yellow, game.gameTitle);
        game.scoreText = UIText("Skor", rt, new Vector2(0f, 110f), new Vector2(480f, 60f), 44f, Color.white, "SKOR: 0");
        game.timeText = UIText("Sure", rt, new Vector2(0f, 55f), new Vector2(480f, 60f), 44f, Color.white, "SÜRE: " + game.duration);
        game.bestText = UIText("Rekor", rt, new Vector2(0f, 5f), new Vector2(480f, 40f), 30f, new Color(1f, 0.9f, 0.6f), "REKOR: 0");
        game.infoText = UIText("Bilgi", rt, new Vector2(0f, -75f), new Vector2(460f, 110f), 25f, Color.white, game.howToPlay);
        game.infoText.textWrappingMode = TextWrappingModes.Normal;

        Image buttonImage = UIImage("Baslat_Butonu", rt, new Vector2(0f, -178f), new Vector2(260f, 80f), new Color(0.2f, 0.7f, 0.2f));
        Button button = buttonImage.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        buttonImage.gameObject.AddComponent<Outline>().effectDistance = new Vector2(4f, -4f);

        game.startButton = button;
        game.startButtonImage = buttonImage;
        game.startButtonLabel = UIText("Yazi", buttonImage.rectTransform, Vector2.zero, new Vector2(260f, 80f), 40f, Color.white, "BAŞLAT");

        // Paneli tutan direk.
        Vector3 back3 = rt.rotation * Vector3.forward * 0.04f;
        float poleHeight = pos.y - 0.46f - Y0;
        Prim(PrimitiveType.Cylinder, "Panel_Ayagi", parent, new Vector3(pos.x, Y0 + poleHeight / 2f, pos.z) + back3,
            new Vector3(0.06f, poleHeight / 2f, 0.06f), Mat("Tahta", Wood));
        Prim(PrimitiveType.Cube, "Panel_Arka", parent, pos + back3, new Vector3(1.02f, 0.94f, 0.02f), Mat("Tahta", Wood), true, new Vector3(0f, yaw, 0f));

        EditorUtility.SetDirty(game);
    }

    static Image UIImage(string name, RectTransform parent, Vector2 anchored, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = anchored;
        rt.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    static TextMeshProUGUI UIText(string name, RectTransform parent, Vector2 anchored, Vector2 size, float fontSize, Color color, string text)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = anchored;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }

    // ---------------------------------------------------------------- PARÇACIKLAR

    static ParticleSystem MakeEffect(string name, Color[] colors, int count, float speed, float size, float gravity, bool withRing)
    {
        Material mat = ParticleMat();

        GameObject go = new GameObject(name);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Konfeti: yukarı doğru saçılan, dönen renkli kareler.
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size * 1.3f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;
        main.startColor = RandomColors(colors);

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 40f;
        shape.radius = 0.15f;
        shape.rotation = new Vector3(-90f, 0f, 0f); // yukarı

        var rotation = ps.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-8f, 8f);

        var velocity = ps.limitVelocityOverLifetime;
        velocity.enabled = true;
        velocity.limit = speed;
        velocity.dampen = 0.15f;

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.7f, 1f), new Keyframe(1f, 0f)));

        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;

        if (withRing)
        {
            // Pota çevresinde dışa açılan altın halka + parlayan kıvılcımlar.
            GameObject ringGo = new GameObject("Halka");
            ringGo.transform.SetParent(go.transform, false);
            ParticleSystem ring = ringGo.AddComponent<ParticleSystem>();
            ring.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var rm = ring.main;
            rm.duration = 0.3f;
            rm.loop = false;
            rm.startLifetime = 0.7f;
            rm.startSpeed = 3f;
            rm.startSize = 0.09f;
            rm.simulationSpace = ParticleSystemSimulationSpace.World;
            rm.startColor = RandomColors(new[] { Yellow, Orange, Color.white });

            var re = ring.emission;
            re.rateOverTime = 0f;
            re.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)48), new ParticleSystem.Burst(0.15f, (short)32) });

            var rs = ring.shape;
            rs.shapeType = ParticleSystemShapeType.Circle;
            rs.radius = 0.3f;
            rs.radiusThickness = 0f;
            rs.rotation = new Vector3(90f, 0f, 0f); // yatay halka

            var rsize = ring.sizeOverLifetime;
            rsize.enabled = true;
            rsize.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            ringGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
        }

        string path = PrefabFolder + "/" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<ParticleSystem>();
    }

    static ParticleSystem.MinMaxGradient RandomColors(Color[] colors)
    {
        Gradient g = new Gradient();
        g.mode = GradientMode.Fixed;
        GradientColorKey[] keys = new GradientColorKey[Mathf.Min(colors.Length, 8)];
        for (int i = 0; i < keys.Length; i++)
            keys[i] = new GradientColorKey(colors[i], (i + 1f) / keys.Length);
        g.SetKeys(keys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });

        return new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
    }

    static Material ParticleMat()
    {
        string path = MatFolder + "/Konfeti.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    // ---------------------------------------------------------------- YARDIMCILAR

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }

    static Material Mat(string name, Color color)
    {
        string path = MatFolder + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Smoothness", 0.15f);
        EditorUtility.SetDirty(m);
        return m;
    }

    static Transform Group(string name, Transform parent, Vector3 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        return go.transform;
    }

    // Ebeveynlerin dönüşü/ölçeği hep birim olduğu için dünya konumu ve ölçeği doğrudan verilir.
    static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material mat,
        bool collider = true, Vector3? euler = null)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(euler ?? Vector3.zero);
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!collider)
            Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material mat, bool collider = true)
    {
        return Prim(PrimitiveType.Cube, name, parent, position, scale, mat, collider);
    }

    static void Sign(string name, Transform parent, string text, Vector3 position, float yaw, float size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.rectTransform.sizeDelta = new Vector2(6f, 1f);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    // Basit masa: üst tabla + 4 bacak. Tablanın üst yüzey yüksekliğini döndürür.
    static float Table(string name, Transform parent, Vector3 center, Vector2 size, float height, Material legMat, Material topMat)
    {
        Transform t = Group(name, parent, center);
        float topY = Y0 + height;
        Box("Tabla", t, new Vector3(center.x, topY - 0.03f, center.z), new Vector3(size.x, 0.06f, size.y), topMat != null ? topMat : legMat);
        for (int i = 0; i < 4; i++)
        {
            float x = center.x + (i % 2 == 0 ? -1f : 1f) * (size.x / 2f - 0.05f);
            float z = center.z + (i < 2 ? -1f : 1f) * (size.y / 2f - 0.05f);
            Box("Bacak_" + i, t, new Vector3(x, Y0 + (height - 0.06f) / 2f, z), new Vector3(0.06f, height - 0.06f, 0.06f), legMat);
        }

        if (topMat != null)
            Lips(t, new Vector3(center.x, topY, center.z), new Vector2(size.x, size.y), legMat);

        return topY;
    }

    // Tezgah üstünde topların yuvarlanıp düşmemesi için alçak kenarlar.
    static void Lips(Transform parent, Vector3 topCenter, Vector2 size, Material mat)
    {
        const float h = 0.05f, w = 0.02f;
        float y = topCenter.y + h / 2f;
        Box("Kenar_1", parent, new Vector3(topCenter.x, y, topCenter.z - size.y / 2f), new Vector3(size.x, h, w), mat);
        Box("Kenar_2", parent, new Vector3(topCenter.x, y, topCenter.z + size.y / 2f), new Vector3(size.x, h, w), mat);
        Box("Kenar_3", parent, new Vector3(topCenter.x - size.x / 2f, y, topCenter.z), new Vector3(w, h, size.y), mat);
        Box("Kenar_4", parent, new Vector3(topCenter.x + size.x / 2f, y, topCenter.z), new Vector3(w, h, size.y), mat);
    }

    // Kırmızı-beyaz çizgili tente, iki direk ve tabela. alongZ: tente Z ekseni boyunca uzanır (oyuncu +X'e bakar).
    static void Awning(Transform parent, Vector3 center, bool alongZ, float length, string title)
    {
        Transform t = Group("Tente", parent, center);
        Material red = Mat("Kirmizi", Red);
        Material white = Mat("Beyaz", White);
        Material wood = Mat("Tahta", Wood);
        float y = Y0 + 2.65f;

        Vector3 axis = alongZ ? Vector3.forward : Vector3.right;
        Vector3 toPlayer = alongZ ? Vector3.left : Vector3.back;

        for (int s = 0; s < 2; s++)
        {
            Vector3 p = center + axis * ((s == 0 ? -1f : 1f) * (length / 2f + 0.05f));
            Prim(PrimitiveType.Cylinder, "Direk_" + s, t, new Vector3(p.x, Y0 + 1.325f, p.z), new Vector3(0.08f, 1.325f, 0.08f), wood);
        }

        const int stripes = 8;
        float w = length / stripes;
        for (int i = 0; i < stripes; i++)
        {
            Vector3 p = center + axis * (-length / 2f + w * (i + 0.5f));
            Vector3 scale = alongZ ? new Vector3(0.7f, 0.15f, w) : new Vector3(w, 0.15f, 0.7f);
            Box("Serit_" + i, t, new Vector3(p.x, y, p.z), scale, i % 2 == 0 ? red : white, false);
        }

        Vector3 signPos = center + toPlayer * 0.34f;
        Vector3 signScale = alongZ ? new Vector3(0.04f, 0.42f, length * 0.6f) : new Vector3(length * 0.6f, 0.42f, 0.04f);
        Box("Tabela", t, new Vector3(signPos.x, y + 0.3f, signPos.z), signScale, Mat("Tabela", Yellow), false);

        Vector3 textPos = signPos + toPlayer * 0.03f;
        Sign("Tabela_Yazi", t, title, new Vector3(textPos.x, y + 0.3f, textPos.z), alongZ ? 90f : 0f, 3f, Red);
    }

    static void Line(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        Material line = AssetDatabase.LoadAssetAtPath<Material>("Assets/Arcade/Materials/Court_Line.mat");
        Box(name, parent, position, scale, line != null ? line : Mat("Beyaz", White), false);
    }

    static void Lamp(string name, Transform parent, Vector3 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 6f;
        light.intensity = 2.5f;
        light.color = new Color(1f, 0.92f, 0.8f);
        light.shadows = LightShadows.None;
    }

    static void Ball(Transform parent, string name, Vector3 position, float diameter, Material mat)
    {
        GameObject go = Prim(PrimitiveType.Sphere, name, parent, position, Vector3.one * diameter, mat);

        string pmPath = MatFolder + "/Top_Zipla.physicMaterial";
        PhysicsMaterial bouncy = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(pmPath);
        if (bouncy == null)
        {
            bouncy = new PhysicsMaterial("Top_Zipla") { bounciness = 0.45f, dynamicFriction = 0.5f, staticFriction = 0.5f };
            AssetDatabase.CreateAsset(bouncy, pmPath);
        }
        go.GetComponent<Collider>().sharedMaterial = bouncy;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.15f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.angularDamping = 0.3f;

        go.AddComponent<XRGrabInteractable>();
        go.AddComponent<AutoReturnBall>();
    }
}
