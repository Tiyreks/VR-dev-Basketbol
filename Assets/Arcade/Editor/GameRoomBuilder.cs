using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Menüler:
//   Arcade > Odayı Güncelle  -> var olan odayı YERİNDE günceller (tabelalar, renkler ve elle yapılan
//                               diğer sanatsal değişiklikler korunur). Normalde bunu kullanın.
//   Arcade > Oyun Odasını Sıfırdan Kur -> "OYUN_ODASI"yı silip baştan kurar (elle yapılan sahne
//                               değişiklikleri kaybolur; malzeme renkleri korunur).
// Malzemeler (Assets/Arcade/Materials/Oda) sadece yoksa oluşturulur; var olanın rengi asla değiştirilmez.
[InitializeOnLoad]
public static class GameRoomBuilder
{
    const string RootName = "OYUN_ODASI";
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    const string MatFolder = "Assets/Arcade/Materials/Oda";
    const string PrefabFolder = "Assets/Arcade/Prefabs";
    const float Y0 = -0.15f; // zemin yüksekliği
    const float PanelScale = 0.0024f;

    // Kapı boşluğu (güney duvarı, z = -6.1).
    const float DoorLeft = 3.85f, DoorRight = 5.15f, DoorHeight = 2.25f, SouthZ = -6.1f;

    // Kutu Devirme atış tezgahının X konumu (masa 5.9'da).
    const float CanCounterX = 3.8f;

    static readonly Color Wood = new Color(0.55f, 0.35f, 0.18f);
    static readonly Color Red = new Color(0.85f, 0.15f, 0.12f);
    static readonly Color White = new Color(0.95f, 0.95f, 0.92f);
    static readonly Color Blue = new Color(0.15f, 0.35f, 0.75f);
    static readonly Color DarkBlue = new Color(0.1f, 0.18f, 0.4f);
    static readonly Color LightBlue = new Color(0.4f, 0.7f, 0.95f);
    static readonly Color Yellow = new Color(1f, 0.85f, 0.1f);
    static readonly Color Orange = new Color(1f, 0.5f, 0.05f);
    static readonly Color Green = new Color(0.2f, 0.55f, 0.25f);
    static readonly Color Purple = new Color(0.35f, 0.1f, 0.6f);

    static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

    static GameRoomBuilder()
    {
        EditorApplication.delayCall += CheckRequest;
    }

    // Proje klasörüne "OyunOdasi.upgrade" (yerinde güncelle) veya "OyunOdasi.request" (sıfırdan kur)
    // dosyası konursa derlemeden sonra otomatik çalışır ve dosya silinir.
    static void CheckRequest()
    {
        string upgrade = Path.Combine(ProjectRoot, "OyunOdasi.upgrade");
        string build = Path.Combine(ProjectRoot, "OyunOdasi.request");
        if (!File.Exists(upgrade) && !File.Exists(build))
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += CheckRequest;
            return;
        }

        if (File.Exists(upgrade))
        {
            File.Delete(upgrade);
            Upgrade();
        }
        else
        {
            File.Delete(build);
            Build();
        }
    }

    static bool PrepareScene(out Scene scene)
    {
        scene = SceneManager.GetActiveScene();

        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[OyunOdasi] Play modundayken çalışmaz.");
            return false;
        }

        if (scene.path != ScenePath)
        {
            if (scene.isDirty)
            {
                Debug.LogWarning("[OyunOdasi] Önce açık sahneyi kaydedin, sonra tekrar deneyin.");
                return false;
            }
            scene = EditorSceneManager.OpenScene(ScenePath);
        }

        EnsureFolder("Assets/Arcade", "Prefabs");
        EnsureFolder("Assets/Arcade/Materials", "Oda");
        return true;
    }

    static void Save(Scene scene, string message)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[OyunOdasi] " + message);
    }

    // ================================================================ YERİNDE GÜNCELLEME

    [MenuItem("Arcade/Odayı Güncelle")]
    public static void Upgrade()
    {
        if (!PrepareScene(out Scene scene))
            return;

        GameObject rootGo = GameObject.Find(RootName);
        if (rootGo == null)
        {
            Build();
            return;
        }
        Transform root = rootGo.transform;

        // Basketbol: sadece panel, duvara (potanın arkasına) taşınır.
        Transform basket = root.Find("BASKETBOL");
        if (basket != null)
        {
            DeleteChildren(basket, "Panel", "Panel_Ayagi", "Panel_Arka");
            BuildPanel(basket.GetComponent<MiniGame>(), basket, BasketPanelPos, -90f, new Color(0.45f, 0.25f, 0.1f));
        }

        // Ördek Avı: az, büyük, yavaş ve yakın ördekler + arka panoda panel.
        Transform duck = root.Find("ORDEK_AVI");
        if (duck != null)
        {
            DeleteChildren(duck, "Panel", "Panel_Ayagi", "Panel_Arka", "Ordekler", "Ray_", "Dalga_");
            BuildDuckTargets(duck);
            DuckGame dg = duck.GetComponent<DuckGame>();
            dg.howToPlay = DuckHowTo;
            BuildPanel(dg, duck, DuckPanelPos, 0f, new Color(0.1f, 0.3f, 0.55f));
        }

        // Kutu Devirme: tezgah kuleye yaklaşır, kutular büyür/hafifler + perdede panel.
        Transform can = root.Find("KUTU_DEVIRME");
        if (can != null)
        {
            Transform counter = can.Find("Tezgah");
            if (counter != null)
            {
                float dx = CanCounterX - counter.position.x;
                if (Mathf.Abs(dx) > 0.01f)
                {
                    foreach (Transform child in can)
                    {
                        string n = child.name;
                        if (n == "Tezgah" || n == "Tezgah_Ust" || n == "Tente" || n == "Atis_Cizgisi" ||
                            n.StartsWith("Kenar_") || n.StartsWith("Top_"))
                            child.position += new Vector3(dx, 0f, 0f);
                    }
                }
            }

            DeleteChildren(can, "Panel", "Panel_Ayagi", "Panel_Arka", "Kutular", "Kule_Tepesi");
            CanGame cg = can.GetComponent<CanGame>();
            BuildCanTower(can, cg);
            cg.howToPlay = CanHowTo;
            BuildPanel(cg, can, CanPanelPos, 90f, new Color(0.15f, 0.4f, 0.2f));
        }

        // Kapı: güney duvarında gerçek bir boşluk + açılıp kapanan kapı + arkasında boş oda.
        Transform room = root.Find("Oda");
        if (room != null)
            DeleteChildren(room, "Duvar_Guney", "Supurgelik_G", "Serit_G", "Kapi", "Kapi_Tokmak");
        DeleteChildren(root, "KAPI", "ATARI_MAKINESI");
        BuildDoor(root);
        BuildArcadeMachine(root);

        Save(scene, "Oda yerinde güncellendi (tabelalar ve renkler korundu).");
    }

    // ================================================================ SIFIRDAN KURULUM

    [MenuItem("Arcade/Oyun Odasını Sıfırdan Kur")]
    public static void Build()
    {
        if (!PrepareScene(out Scene scene))
            return;

        GameObject old = GameObject.Find(RootName);
        if (old != null)
            Object.DestroyImmediate(old);

        // Eski, bozuk referanslı zamanlayıcı. Yerine her standın kendi paneli var.
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
        BuildDoor(root);
        BuildBasketball(root, basketFx);
        BuildDuckGame(root, duckFx);
        BuildCanGame(root, canFx);
        BuildArcadeMachine(root);

        Save(scene, "Oyun odası sıfırdan kuruldu ve sahne kaydedildi.");
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
        Box("Duvar_Bati", room, new Vector3(-7.1f, cy, 1f), new Vector3(0.2f, h, 14.4f), wall2);
        Box("Duvar_Dogu", room, new Vector3(7.1f, cy, 1f), new Vector3(0.2f, h, 14.4f), wall2);
        // Güney duvarı kapı boşluğuyla birlikte BuildDoor'da kurulur.

        float by = Y0 + 0.08f;
        Box("Supurgelik_K", room, new Vector3(0f, by, 7.98f), new Vector3(14f, 0.16f, 0.04f), board, false);
        Box("Supurgelik_B", room, new Vector3(-6.98f, by, 1f), new Vector3(0.04f, 0.16f, 14f), board, false);
        Box("Supurgelik_D", room, new Vector3(6.98f, by, 1f), new Vector3(0.04f, 0.16f, 14f), board, false);

        Box("Serit_B", room, new Vector3(-6.98f, Y0 + 1.2f, 1f), new Vector3(0.03f, 0.12f, 14f), Mat("Serit", Red), false);

        Box("Tabela", room, new Vector3(0f, Y0 + 2.45f, -5.97f), new Vector3(4.2f, 0.9f, 0.04f), Mat("Tabela", Yellow), false);
        Sign("Tabela_Yazi", room, "OYUN ODASI", new Vector3(0f, Y0 + 2.45f, -5.94f), 180f, 7f, Red);
    }

    // ---------------------------------------------------------------- KAPI

    static void BuildDoor(Transform root)
    {
        Transform g = Group("KAPI", root, Vector3.zero);
        Material wall = Mat("Duvar", new Color(0.93f, 0.87f, 0.72f));
        Material board = Mat("Supurgelik", new Color(0.3f, 0.2f, 0.12f));
        Material stripe = Mat("Serit", Red);
        Material wood = Mat("Tahta", Wood);
        Material doorMat = Mat("Kapi", Wood);
        Material metal = Mat("Metal", new Color(0.8f, 0.7f, 0.2f));

        const float h = 3.2f, edge = 7.2f;
        float cy = Y0 + h / 2f;

        // Güney duvarı: kapı boşluğunun solu, sağı ve üstü.
        Box("Duvar_Guney_Sol", g, new Vector3((-edge + DoorLeft) / 2f, cy, SouthZ), new Vector3(DoorLeft + edge, h, 0.2f), wall);
        Box("Duvar_Guney_Sag", g, new Vector3((DoorRight + edge) / 2f, cy, SouthZ), new Vector3(edge - DoorRight, h, 0.2f), wall);
        Box("Duvar_Guney_Ust", g, new Vector3((DoorLeft + DoorRight) / 2f, Y0 + (DoorHeight + h) / 2f, SouthZ),
            new Vector3(DoorRight - DoorLeft, h - DoorHeight, 0.2f), wall);

        const float inner = 7f;
        Box("Supurgelik_G_Sol", g, new Vector3((-inner + DoorLeft) / 2f, Y0 + 0.08f, -5.98f), new Vector3(DoorLeft + inner, 0.16f, 0.04f), board, false);
        Box("Supurgelik_G_Sag", g, new Vector3((DoorRight + inner) / 2f, Y0 + 0.08f, -5.98f), new Vector3(inner - DoorRight, 0.16f, 0.04f), board, false);
        Box("Serit_G_Sol", g, new Vector3((-inner + DoorLeft) / 2f, Y0 + 1.2f, -5.98f), new Vector3(DoorLeft + inner, 0.12f, 0.03f), stripe, false);
        Box("Serit_G_Sag", g, new Vector3((DoorRight + inner) / 2f, Y0 + 1.2f, -5.98f), new Vector3(inner - DoorRight, 0.12f, 0.03f), stripe, false);

        // Kasa.
        Box("Kasa_Sol", g, new Vector3(DoorLeft - 0.03f, Y0 + DoorHeight / 2f, SouthZ), new Vector3(0.06f, DoorHeight, 0.26f), wood, false);
        Box("Kasa_Sag", g, new Vector3(DoorRight + 0.03f, Y0 + DoorHeight / 2f, SouthZ), new Vector3(0.06f, DoorHeight, 0.26f), wood, false);
        Box("Kasa_Ust", g, new Vector3((DoorLeft + DoorRight) / 2f, Y0 + DoorHeight + 0.03f, SouthZ), new Vector3(DoorRight - DoorLeft + 0.12f, 0.06f, 0.26f), wood, false);

        // Menteşe (sol kenarda) ve kanat. Kanat odanın içine doğru açılır.
        float width = DoorRight - DoorLeft - 0.05f;
        Transform hinge = Group("Mentese", g, new Vector3(DoorLeft + 0.025f, Y0, SouthZ));
        GameObject leaf = Box("Kanat", hinge, new Vector3(hinge.position.x + width / 2f, Y0 + (DoorHeight - 0.03f) / 2f, SouthZ),
            new Vector3(width, DoorHeight - 0.05f, 0.05f), doorMat);
        Rigidbody rb = leaf.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        XRSimpleInteractable interactable = leaf.AddComponent<XRSimpleInteractable>();

        Box("Kol_Ic", hinge, new Vector3(hinge.position.x + width - 0.1f, Y0 + 1.0f, SouthZ + 0.06f), new Vector3(0.12f, 0.04f, 0.06f), metal, false);
        Box("Kol_Dis", hinge, new Vector3(hinge.position.x + width - 0.1f, Y0 + 1.0f, SouthZ - 0.06f), new Vector3(0.12f, 0.04f, 0.06f), metal, false);
        Box("Kapi_Pencere", hinge, new Vector3(hinge.position.x + width / 2f, Y0 + 1.65f, SouthZ + 0.027f), new Vector3(0.5f, 0.35f, 0.01f), Mat("Su", LightBlue), false);

        Door door = hinge.gameObject.AddComponent<Door>();
        door.leaf = interactable;

        Sign("Kapi_Yazi", g, "KAPIYI TUT: AÇ / KAPAT", new Vector3((DoorLeft + DoorRight) / 2f, Y0 + DoorHeight + 0.2f, -5.98f), 180f, 1.2f, Color.black);

        // Kapının arkasındaki boş oda.
        Transform empty = Group("Bos_Oda", g, Vector3.zero);
        const float back = -8.8f;
        float midX = (DoorLeft + DoorRight) / 2f;
        Box("Bos_Zemin", empty, new Vector3(midX, Y0 - 0.05f, (-6f + back) / 2f), new Vector3(3f, 0.1f, -6f - back), Mat("Bos_Zemin", new Color(0.5f, 0.5f, 0.5f)));
        Box("Duvar_Bati", empty, new Vector3(midX - 1.6f, cy, (SouthZ + back) / 2f), new Vector3(0.2f, h, SouthZ - back), wall);
        Box("Duvar_Dogu", empty, new Vector3(midX + 1.6f, cy, (SouthZ + back) / 2f), new Vector3(0.2f, h, SouthZ - back), wall);
        Box("Duvar_Guney", empty, new Vector3(midX, cy, back - 0.1f), new Vector3(3.4f, h, 0.2f), wall);
    }

    // ---------------------------------------------------------------- ATARİ MAKİNESİ (süs)

    static void BuildArcadeMachine(Transform root)
    {
        // Batı duvarının güney köşesinde, oyuncuya (+X) bakar.
        Transform g = Group("ATARI_MAKINESI", root, new Vector3(-6.55f, Y0, -4.6f));
        Material body = Mat("Atari_Govde", Purple);
        Material black = Mat("Siyah", Color.black);
        Material yellow = Mat("Atari_Sari", Yellow);
        const float z = -4.6f;

        Box("Govde", g, new Vector3(-6.6f, Y0 + 0.85f, z), new Vector3(0.7f, 1.7f, 0.75f), body);
        Box("Cerceve", g, new Vector3(-6.24f, Y0 + 1.35f, z), new Vector3(0.04f, 0.5f, 0.62f), black, false);
        GameObject screen = Box("Ekran", g, new Vector3(-6.215f, Y0 + 1.35f, z), new Vector3(0.01f, 0.42f, 0.54f), UnlitMat("Atari_Ekran", new Color(0.05f, 0.05f, 0.35f)), false);

        Transform center = Group("Ekran_Merkez", g, new Vector3(-6.205f, Y0 + 1.35f, z));
        center.rotation = Quaternion.Euler(0f, 90f, 0f); // yerel X/Y ekran düzleminde, Z oyuncuya doğru
        GameObject pixel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pixel.name = "Piksel";
        Object.DestroyImmediate(pixel.GetComponent<Collider>());
        pixel.transform.SetParent(center, false);
        pixel.transform.localScale = Vector3.one * 0.03f;
        pixel.GetComponent<Renderer>().sharedMaterial = UnlitMat("Atari_Piksel", Color.white);

        GameObject blink = Sign("Jeton_Yazi", g, "JETON AT", new Vector3(-6.2f, Y0 + 1.2f, z), -90f, 1f, Yellow);

        Box("Kontrol_Paneli", g, new Vector3(-6.12f, Y0 + 1.0f, z), new Vector3(0.26f, 0.08f, 0.72f), body);
        Prim(PrimitiveType.Cylinder, "Kol_Cubuk", g, new Vector3(-6.1f, Y0 + 1.1f, z - 0.18f), new Vector3(0.02f, 0.06f, 0.02f), black, false);
        Prim(PrimitiveType.Sphere, "Kol_Top", g, new Vector3(-6.1f, Y0 + 1.17f, z - 0.18f), Vector3.one * 0.06f, Mat("Kirmizi", Red), false);
        Material[] buttons = { Mat("Kirmizi", Red), yellow, Mat("Mavi", Blue) };
        for (int i = 0; i < 3; i++)
            Prim(PrimitiveType.Cylinder, "Tus_" + i, g, new Vector3(-6.1f, Y0 + 1.045f, z + 0.05f + i * 0.1f), new Vector3(0.05f, 0.01f, 0.05f), buttons[i], false);

        Box("Ust_Tabela", g, new Vector3(-6.5f, Y0 + 1.85f, z), new Vector3(0.6f, 0.3f, 0.75f), yellow);
        Sign("Ust_Yazi", g, "ATARİ", new Vector3(-6.19f, Y0 + 1.85f, z), -90f, 2.2f, Red);

        Box("Jeton_Kapagi", g, new Vector3(-6.24f, Y0 + 0.5f, z), new Vector3(0.02f, 0.2f, 0.15f), black, false);
        Box("Jeton_Deligi", g, new Vector3(-6.228f, Y0 + 0.55f, z), new Vector3(0.01f, 0.05f, 0.015f), Mat("Kirmizi", Red), false);
        for (int s = -1; s <= 1; s += 2)
            Box("Yan_Serit_" + s, g, new Vector3(-6.6f, Y0 + 1.5f, z + s * 0.38f), new Vector3(0.6f, 0.06f, 0.01f), yellow, false);

        ArcadeScreen anim = g.gameObject.AddComponent<ArcadeScreen>();
        anim.screen = screen.GetComponent<Renderer>();
        anim.pixel = pixel.transform;
        anim.blinkText = blink;
        anim.bounds = new Vector2(0.25f, 0.19f);
    }

    // ---------------------------------------------------------------- BASKETBOL

    static readonly Vector3 BasketPanelPos = new Vector3(-6.97f, Y0 + 2.15f, 1.3f);

    static void BuildBasketball(Transform root, ParticleSystem fx)
    {
        Transform booth = Group("BASKETBOL", root, new Vector3(-4f, 1f, 0.5f));
        BasketballGame game = booth.gameObject.AddComponent<BasketballGame>();
        game.gameTitle = "BASKETBOL";
        game.howToPlay = "Topu al ve potaya at!\nHer basket 1 puan.";
        game.popupColor = new Color(1f, 0.6f, 0.1f);
        game.hitEffect = fx;

        BasketDetector detector = Object.FindFirstObjectByType<BasketDetector>();
        game.detector = detector;

        // Basket parçacıkları: konfeti + altın halka efekti.
        if (detector != null)
        {
            SerializedObject so = new SerializedObject(detector);
            so.FindProperty("scoreEffectPrefab").objectReferenceValue = fx;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        Material wood = Mat("Tahta", Wood);
        Material white = Mat("Beyaz", White);

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
                float t = i / (float)list.Length * 4f;
                float y, zz;
                if (t < 1f) { y = 2.55f; zz = Mathf.Lerp(-1.3f, 0.4f, t); }
                else if (t < 2f) { y = Mathf.Lerp(2.55f, 3.55f, t - 1f); zz = 0.4f; }
                else if (t < 3f) { y = 3.55f; zz = Mathf.Lerp(0.4f, -1.3f, t - 2f); }
                else { y = Mathf.Lerp(3.55f, 2.55f, t - 3f); zz = -1.3f; }

                GameObject b = Prim(PrimitiveType.Sphere, "Ampul_" + i, bulbs, new Vector3(-5.54f, y, zz), Vector3.one * 0.07f, bulb, false);
                list[i] = b.GetComponent<Renderer>();
            }
            lights.bulbs = list;
            EditorUtility.SetDirty(lights);
        }

        Vector3 rack = new Vector3(-2.9f, 0f, -1.6f);
        float top = Table("Raf", booth, rack, new Vector2(0.5f, 1.0f), 0.8f, wood, white);

        GameObject ball = GameObject.Find("TOPP");
        if (ball != null)
        {
            ball.transform.position = new Vector3(rack.x, top + 0.125f, rack.z - 0.3f);
            AutoReturnBall ar = ball.GetComponent<AutoReturnBall>();
            if (ar == null)
                ar = ball.AddComponent<AutoReturnBall>();
            ar.playBounceSound = false;

            for (int i = 1; i <= 2; i++)
            {
                GameObject copy = Object.Instantiate(ball, booth);
                copy.name = "TOPP (" + (i + 1) + ")";
                copy.transform.position = new Vector3(rack.x, top + 0.125f, rack.z - 0.3f + i * 0.3f);
            }
        }

        Line("Atis_Cizgisi", booth, new Vector3(-2.3f, Y0 + 0.005f, -0.45f), new Vector3(0.06f, 0.01f, 2.4f));
        Lamp("Lamba", booth, new Vector3(-3.8f, 2.6f, 0f));

        BuildPanel(game, booth, BasketPanelPos, -90f, new Color(0.45f, 0.25f, 0.1f));
    }

    // ---------------------------------------------------------------- ÖRDEK AVI

    const string DuckHowTo = "Büyük ördeklere top at!\nAlt sıra 1, üst sıra 2 puan.";
    static readonly Vector3 DuckPanelPos = new Vector3(1f, Y0 + 2.8f, 7.68f);
    const float DuckRailZ = 6.4f;

    static void BuildDuckGame(Transform root, ParticleSystem fx)
    {
        Transform booth = Group("ORDEK_AVI", root, new Vector3(1f, 1f, 6f));
        DuckGame game = booth.gameObject.AddComponent<DuckGame>();
        game.gameTitle = "ÖRDEK AVI";
        game.howToPlay = DuckHowTo;
        game.popupColor = Yellow;
        game.hitEffect = fx;

        Material wood = Mat("Tahta", Wood);
        Material white = Mat("Beyaz", White);
        Material red = Mat("Kirmizi", Red);

        Box("Arka_Pano", booth, new Vector3(1f, Y0 + 1.6f, 7.75f), new Vector3(7.6f, 3.2f, 0.1f), Mat("Lacivert", DarkBlue));
        Box("Yan_Pano_Sol", booth, new Vector3(-2.85f, Y0 + 1.6f, 6.5f), new Vector3(0.1f, 3.2f, 2.6f), Mat("Lacivert", DarkBlue));
        Box("Yan_Pano_Sag", booth, new Vector3(4.85f, Y0 + 1.6f, 6.5f), new Vector3(0.1f, 3.2f, 2.6f), Mat("Lacivert", DarkBlue));

        BuildDuckTargets(booth);

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

        BuildPanel(game, booth, DuckPanelPos, 0f, new Color(0.1f, 0.3f, 0.55f));
    }

    // Süper kolay: 2 sıra x 2 ördek, 1.7 kat büyük, yavaş, tezgaha daha yakın.
    static void BuildDuckTargets(Transform booth)
    {
        Material wood = Mat("Tahta", Wood);
        Material water = Mat("Su", LightBlue);
        const float size = 1.7f;

        Transform ducks = Group("Ordekler", booth, new Vector3(1f, 0f, DuckRailZ));
        float[] speeds = { 0.45f, 0.6f };
        const float track = 4.8f;
        const float gap = 2.4f;

        for (int row = 0; row < 2; row++)
        {
            float railY = Y0 + 0.6f + row * 0.75f;

            Box("Ray_" + row, booth, new Vector3(1f, railY - 0.04f, DuckRailZ), new Vector3(5.8f, 0.08f, 0.14f), wood);
            Box("Dalga_" + row, booth, new Vector3(1f, railY + 0.02f, DuckRailZ - 0.32f), new Vector3(5.8f, 0.2f, 0.03f), water);
            for (int w = 0; w < 7; w++)
                Prim(PrimitiveType.Sphere, "Dalga_Tepe_" + row + "_" + w, booth,
                    new Vector3(-1.7f + w * 0.9f, railY + 0.12f, DuckRailZ - 0.32f), new Vector3(0.36f, 0.14f, 0.03f), water, false);

            for (int k = 0; k < 2; k++)
            {
                float minX = -track / 2f + k * gap;
                float maxX = minX + track - gap;
                BuildDuck(ducks, "Ordek_" + row + "_" + k, new Vector3(1f + minX, railY, DuckRailZ),
                    minX, maxX, speeds[row], row * 1.2f, row + 1, size);
            }
        }
    }

    static void BuildDuck(Transform parent, string name, Vector3 pos, float minX, float maxX, float speed, float phase, int points, float s)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Çarpışma kutusu görünenden biraz büyük: ıskalamak zor olsun.
        BoxCollider box = go.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.32f, 0f) * s;
        box.size = new Vector3(0.52f, 0.46f, 0.36f) * s;

        Transform pivot = Group("Pivot", go.transform, pos);

        Material yellow = Mat("Ordek_Sari", Yellow);
        Material wing = Mat("Ordek_Kanat", new Color(0.9f, 0.7f, 0.05f));
        Material orange = Mat("Ordek_Gaga", Orange);
        Material black = Mat("Siyah", Color.black);
        Material red = Mat("Kirmizi", Red);
        Material white = Mat("Beyaz", White);

        Prim(PrimitiveType.Cylinder, "Cubuk", pivot, pos + new Vector3(0f, 0.08f, 0f) * s, new Vector3(0.03f, 0.08f, 0.03f) * s, Mat("Tahta", Wood), false);

        Vector3 disc = new Vector3(0f, 0.3f, -0.14f) * s;
        Vector3 flat = new Vector3(90f, 0f, 0f);
        Prim(PrimitiveType.Cylinder, "Hedef_1", pivot, pos + disc, new Vector3(0.15f, 0.004f, 0.15f) * s, red, false, flat);
        Prim(PrimitiveType.Cylinder, "Hedef_2", pivot, pos + disc + new Vector3(0f, 0f, -0.004f * s), new Vector3(0.09f, 0.004f, 0.09f) * s, white, false, flat);
        Prim(PrimitiveType.Cylinder, "Hedef_3", pivot, pos + disc + new Vector3(0f, 0f, -0.008f * s), new Vector3(0.035f, 0.004f, 0.035f) * s, red, false, flat);

        Transform model = Group("Model", pivot, pos);
        Prim(PrimitiveType.Sphere, "Govde", model, pos + new Vector3(0f, 0.28f, 0f) * s, new Vector3(0.22f, 0.22f, 0.34f) * s, yellow, false);
        Prim(PrimitiveType.Sphere, "Kafa", model, pos + new Vector3(0f, 0.45f, 0.12f) * s, Vector3.one * 0.17f * s, yellow, false);
        Prim(PrimitiveType.Cube, "Gaga", model, pos + new Vector3(0f, 0.44f, 0.23f) * s, new Vector3(0.08f, 0.035f, 0.09f) * s, orange, false);
        Prim(PrimitiveType.Sphere, "Goz_L", model, pos + new Vector3(-0.07f, 0.48f, 0.17f) * s, Vector3.one * 0.035f * s, black, false);
        Prim(PrimitiveType.Sphere, "Goz_R", model, pos + new Vector3(0.07f, 0.48f, 0.17f) * s, Vector3.one * 0.035f * s, black, false);
        Prim(PrimitiveType.Sphere, "Kuyruk", model, pos + new Vector3(0f, 0.35f, -0.17f) * s, new Vector3(0.08f, 0.1f, 0.1f) * s, yellow, false);
        Prim(PrimitiveType.Sphere, "Kanat_L", model, pos + new Vector3(-0.1f, 0.3f, -0.02f) * s, new Vector3(0.04f, 0.12f, 0.2f) * s, wing, false);
        Prim(PrimitiveType.Sphere, "Kanat_R", model, pos + new Vector3(0.1f, 0.3f, -0.02f) * s, new Vector3(0.04f, 0.12f, 0.2f) * s, wing, false);

        Duck duck = go.AddComponent<Duck>();
        duck.minX = minX;
        duck.maxX = maxX;
        duck.speed = speed;
        duck.phase = phase;
        duck.points = points;
        duck.pivot = pivot;
        duck.model = model;
        duck.hitSpeed = 0.3f;
        duck.downTime = 1.2f;
    }

    // ---------------------------------------------------------------- KUTU DEVİRME

    const string CanHowTo = "Kutuları masadan düşür!\nHer kutu 1 puan, hepsi düşerse +3.";
    static readonly Vector3 CanPanelPos = new Vector3(6.64f, Y0 + 2.5f, 1f);
    static readonly Vector3 CanTablePos = new Vector3(5.9f, 0f, 1f);
    const float CanTableHeight = 0.9f;

    static void BuildCanGame(Transform root, ParticleSystem fx)
    {
        Transform booth = Group("KUTU_DEVIRME", root, new Vector3(4.5f, 1f, 1f));
        CanGame game = booth.gameObject.AddComponent<CanGame>();
        game.gameTitle = "KUTU DEVİRME";
        game.howToPlay = CanHowTo;
        game.popupColor = new Color(0.3f, 1f, 0.4f);
        game.hitEffect = fx;

        Material wood = Mat("Tahta", Wood);
        Material white = Mat("Beyaz", White);

        Table("Kutu_Masasi", booth, CanTablePos, new Vector2(0.8f, 1.3f), CanTableHeight, wood, null);
        Box("Masa_Ortusu", booth, new Vector3(5.49f, Y0 + 0.6f, 1f), new Vector3(0.02f, 0.6f, 1.3f), Mat("Kirmizi", Red));
        Box("Arka_Perde", booth, new Vector3(6.75f, Y0 + 1.5f, 1f), new Vector3(0.1f, 3f, 3.4f), Mat("Yesil", Green));

        BuildCanTower(booth, game);

        float x = CanCounterX;
        Box("Tezgah", booth, new Vector3(x, Y0 + 0.5f, 1f), new Vector3(0.5f, 1.0f, 2.6f), Mat("Mavi", Blue));
        float top = Y0 + 1.02f;
        Box("Tezgah_Ust", booth, new Vector3(x, top - 0.01f, 1f), new Vector3(0.56f, 0.02f, 2.7f), white);
        Lips(booth, new Vector3(x, top, 1f), new Vector2(0.56f, 2.7f), wood);

        Color[] ballColors = { Yellow, Orange, Red, Green, LightBlue };
        for (int i = 0; i < 5; i++)
            Ball(booth, "Top_" + i, new Vector3(x, top + 0.065f, 1f - 0.9f + i * 0.45f), 0.12f, Mat("KTop_" + i, ballColors[i]));

        Awning(booth, new Vector3(x, 0f, 1f), true, 2.8f, "KUTU DEVİRME");
        Line("Atis_Cizgisi", booth, new Vector3(x - 0.6f, Y0 + 0.005f, 1f), new Vector3(0.06f, 0.01f, 2.8f));
        Lamp("Lamba", booth, new Vector3(4.6f, 2.6f, 1f));

        BuildPanel(game, booth, CanPanelPos, 90f, new Color(0.15f, 0.4f, 0.2f));
    }

    // Kolay kule: büyük, hafif 3-2-1 kutu. Masadan düşen kutu (yere yakın) sayılır.
    static void BuildCanTower(Transform booth, CanGame game)
    {
        float tableTop = Y0 + CanTableHeight;
        Transform cans = Group("Kutular", booth, Vector3.zero);
        Material[] canMats = { Mat("Kutu_Kirmizi", Red), Mat("Kutu_Mavi", Blue), Mat("Kutu_Yesil", Green) };
        Material band = Mat("Kutu_Etiket", White);
        const float canH = 0.24f, canD = 0.18f, spacing = 0.19f;
        int index = 0;
        for (int row = 0; row < 3; row++)
        {
            int count = 3 - row;
            for (int j = 0; j < count; j++)
            {
                Vector3 p = new Vector3(CanTablePos.x, tableTop + canH / 2f + row * canH + 0.001f, CanTablePos.z + (j - (count - 1) / 2f) * spacing);
                BuildCan(cans, "Kutu_" + index, p, canD, canH, canMats[index % canMats.Length], band);
                index++;
            }
        }

        game.towerTop = Group("Kule_Tepesi", booth, new Vector3(CanTablePos.x, tableTop + 3 * canH, CanTablePos.z));
        game.groundY = Y0 + 0.35f;
        game.allDownBonus = 3;
        EditorUtility.SetDirty(game);
    }

    static void BuildCan(Transform parent, string name, Vector3 pos, float diameter, float height, Material mat, Material bandMat)
    {
        GameObject can = Prim(PrimitiveType.Cylinder, name, parent, pos, new Vector3(diameter, height / 2f, diameter), mat, false);
        can.AddComponent<BoxCollider>();

        Rigidbody rb = can.AddComponent<Rigidbody>();
        rb.mass = 0.1f; // hafif: kolay devrilir
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

    // ---------------------------------------------------------------- DÜNYA PANELİ (UI, butonsuz)

    // Oyunun arkasında (duvarda / panoda), oyuncuya bakan bilgi paneli. Skor otomatik sayılır.
    static void BuildPanel(MiniGame game, Transform parent, Vector3 pos, float yaw, Color bg)
    {
        GameObject go = new GameObject("Panel", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(500f, 400f);
        rt.position = pos;
        rt.rotation = Quaternion.Euler(0f, yaw, 0f);
        rt.localScale = Vector3.one * PanelScale;

        Image back = UIImage("Arkaplan", rt, Vector2.zero, new Vector2(500f, 400f), bg);
        Outline outline = back.gameObject.AddComponent<Outline>();
        outline.effectColor = Yellow;
        outline.effectDistance = new Vector2(8f, -8f);

        game.titleText = UIText("Baslik", rt, new Vector2(0f, 150f), new Vector2(480f, 70f), 50f, Yellow, game.gameTitle);
        game.scoreText = UIText("Skor", rt, new Vector2(0f, 82f), new Vector2(480f, 60f), 48f, Color.white, "SKOR: 0");
        game.timeText = UIText("Sure", rt, new Vector2(0f, 25f), new Vector2(480f, 60f), 44f, Color.white, "SÜRE: " + game.duration);
        game.bestText = UIText("Rekor", rt, new Vector2(0f, -25f), new Vector2(480f, 40f), 30f, new Color(1f, 0.9f, 0.6f), "REKOR: 0");
        game.infoText = UIText("Bilgi", rt, new Vector2(0f, -120f), new Vector2(460f, 140f), 26f, Color.white, game.howToPlay);
        game.infoText.textWrappingMode = TextWrappingModes.Normal;

        Vector3 behind = rt.rotation * Vector3.forward * 0.015f;
        Prim(PrimitiveType.Cube, "Panel_Arka", parent, pos + behind, new Vector3(500f * PanelScale + 0.04f, 400f * PanelScale + 0.04f, 0.02f),
            Mat("Tahta", Wood), false, new Vector3(0f, yaw, 0f));

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
        image.raycastTarget = false;
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
        shape.rotation = new Vector3(-90f, 0f, 0f);

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
            rs.rotation = new Vector3(90f, 0f, 0f);

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

    // Malzeme yoksa verilen renkle oluşturur; varsa olduğu gibi (elle verilen renkle) kullanır.
    static Material Mat(string name, Color color)
    {
        return MatWithShader(name, color, "Universal Render Pipeline/Lit");
    }

    static Material UnlitMat(string name, Color color)
    {
        return MatWithShader(name, color, "Universal Render Pipeline/Unlit");
    }

    static Material MatWithShader(string name, Color color, string shader)
    {
        string path = MatFolder + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find(shader));
            m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Smoothness"))
                m.SetFloat("_Smoothness", 0.15f);
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    // Verilen isimle başlayan doğrudan çocukları siler.
    static void DeleteChildren(Transform parent, params string[] prefixes)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            foreach (string prefix in prefixes)
            {
                if (child.name == prefix || (prefix.EndsWith("_") && child.name.StartsWith(prefix)))
                {
                    Object.DestroyImmediate(child.gameObject);
                    break;
                }
            }
        }
    }

    static Transform Group(string name, Transform parent, Vector3 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        return go.transform;
    }

    // Ebeveynlerin dönüşü/ölçeği birim olduğu için dünya konumu ve ölçeği doğrudan verilir.
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

    static GameObject Sign(string name, Transform parent, string text, Vector3 position, float yaw, float size, Color color)
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
        return go;
    }

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

    static void Lips(Transform parent, Vector3 topCenter, Vector2 size, Material mat)
    {
        const float h = 0.05f, w = 0.02f;
        float y = topCenter.y + h / 2f;
        Box("Kenar_1", parent, new Vector3(topCenter.x, y, topCenter.z - size.y / 2f), new Vector3(size.x, h, w), mat);
        Box("Kenar_2", parent, new Vector3(topCenter.x, y, topCenter.z + size.y / 2f), new Vector3(size.x, h, w), mat);
        Box("Kenar_3", parent, new Vector3(topCenter.x - size.x / 2f, y, topCenter.z), new Vector3(w, h, size.y), mat);
        Box("Kenar_4", parent, new Vector3(topCenter.x + size.x / 2f, y, topCenter.z), new Vector3(w, h, size.y), mat);
    }

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
