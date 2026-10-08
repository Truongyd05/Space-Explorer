using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tự động dựng toàn bộ game: prefab (laser, thiên thạch, sao, hiệu ứng) và 3 scene
/// (MainMenu, Gameplay, EndGame), rồi đăng ký vào Build Settings.
/// Chạy tự động lần đầu mở project; có thể chạy lại bằng menu  Space Explorer > Build Game.
/// </summary>
[InitializeOnLoad]
public static class SpaceExplorerBuilder
{
    const string Art = "Assets/Art/";
    const string PrefabDir = "Assets/Prefabs/";
    const string SceneDir = "Assets/Scenes/";

    static readonly Color Navy = new Color(0.03f, 0.04f, 0.12f);
    static readonly Color Cyan = new Color(0.4f, 0.9f, 1f);
    static readonly Color Gold = new Color(1f, 0.85f, 0.2f);

    static SpaceExplorerBuilder()
    {
        // Lần đầu mở project (chưa có scene) thì tự dựng game.
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(SceneDir + "MainMenu.unity") && !EditorApplication.isPlayingOrWillChangePlaymode)
                Build();
        };
    }

    [MenuItem("Space Explorer/Build Game")]
    public static void Build()
    {
        AssetDatabase.Refresh();
        Directory.CreateDirectory(PrefabDir);
        Directory.CreateDirectory(SceneDir);

        // Đảm bảo mọi sprite đã import với đúng cấu hình
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" }))
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceSynchronousImport);

        GameObject explosion = MakeExplosionPrefab();
        GameObject sparkle = MakeSparklePrefab();
        GameObject laser = MakeLaserPrefab();
        GameObject asteroid = MakeAsteroidPrefab(explosion);
        GameObject star = MakeStarPrefab(sparkle);

        BuildMainMenu();
        BuildGameplay(laser, explosion, asteroid, star);
        BuildEndGame();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SceneDir + "MainMenu.unity", true),
            new EditorBuildSettingsScene(SceneDir + "Gameplay.unity", true),
            new EditorBuildSettingsScene(SceneDir + "EndGame.unity", true),
        };

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(SceneDir + "MainMenu.unity");
        Debug.Log("Space Explorer: đã dựng xong 3 scene. Mở MainMenu và bấm Play!");
    }

    // ------------------------------------------------------------------ Prefabs

    static Sprite Spr(string relPath)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + relPath);
        if (s == null) Debug.LogError("Không tìm thấy sprite: " + relPath);
        return s;
    }

    static GameObject SavePrefab(GameObject go, string name)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject MakeExplosionPrefab()
    {
        GameObject go = new GameObject("Explosion");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 20;
        AnimatedEffect fx = go.AddComponent<AnimatedEffect>();
        fx.frames = new[]
        {
            Spr("Sprites/Explosion_01.png"), Spr("Sprites/Explosion_02.png"),
            Spr("Sprites/Explosion_03.png"), Spr("Sprites/Explosion_04.png"),
        };
        fx.duration = 0.55f;
        fx.startScale = 0.6f;
        fx.endScale = 1.1f;
        fx.fadeOut = false;
        return SavePrefab(go, "Explosion");
    }

    static GameObject MakeSparklePrefab()
    {
        GameObject go = new GameObject("CollectSparkle");
        go.AddComponent<SpriteRenderer>().sortingOrder = 20;
        AnimatedEffect fx = go.AddComponent<AnimatedEffect>();
        fx.frames = new[] { Spr("Sprites/Collect_Sparkle.png") };
        fx.duration = 0.4f;
        fx.startScale = 0.5f;
        fx.endScale = 1.3f;
        fx.fadeOut = true;
        return SavePrefab(go, "CollectSparkle");
    }

    static GameObject MakeLaserPrefab()
    {
        GameObject go = new GameObject("Laser");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("Sprites/Laser.png");
        sr.sortingOrder = 8;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        CapsuleCollider2D col = go.AddComponent<CapsuleCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.25f, 0.8f);

        go.AddComponent<Laser>();
        return SavePrefab(go, "Laser");
    }

    static GameObject MakeAsteroidPrefab(GameObject explosion)
    {
        GameObject go = new GameObject("Asteroid");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("Sprites/Asteroid_Gray.png");
        sr.sortingOrder = 5;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        Asteroid a = go.AddComponent<Asteroid>();
        a.sprites = new[]
        {
            Spr("Sprites/Asteroid_Blue.png"), Spr("Sprites/Asteroid_Brown.png"), Spr("Sprites/Asteroid_Gray.png"),
        };
        a.explosionPrefab = explosion;
        return SavePrefab(go, "Asteroid");
    }

    static GameObject MakeStarPrefab(GameObject sparkle)
    {
        GameObject go = new GameObject("Star");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("Sprites/Star.png");
        sr.sortingOrder = 4;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        go.AddComponent<Star>().sparklePrefab = sparkle;
        return SavePrefab(go, "Star");
    }

    // ------------------------------------------------------------------ Scenes

    static Scene NewScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Navy;
        camGo.transform.position = new Vector3(0, 0, -10);

        GameObject bg = new GameObject("Background");
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("Backgrounds/Space_Nebula.png");
        sr.sortingOrder = -100;

        return scene;
    }

    static void BuildMainMenu()
    {
        NewScene();
        Canvas canvas = MakeCanvas();
        MakeEventSystem();

        MainMenuUI menu = new GameObject("MainMenuUI").AddComponent<MainMenuUI>();

        MakeText(canvas.transform, "Title", "SPACE EXPLORER", 120, Gold, new Vector2(0, 330), new Vector2(1400, 160));
        MakeText(canvas.transform, "Subtitle", "Avoid the asteroids. Collect the stars.", 40, Cyan, new Vector2(0, 230), new Vector2(1200, 60));

        Button play = MakeButton(canvas.transform, "PlayButton", "PLAY", new Vector2(0, 60));
        Button info = MakeButton(canvas.transform, "InstructionsButton", "INSTRUCTIONS", new Vector2(0, -170));
        Button quit = MakeButton(canvas.transform, "QuitButton", "QUIT", new Vector2(0, -400));

        // Instructions panel (ẩn mặc định)
        GameObject panel = new GameObject("InstructionsPanel", typeof(RectTransform));
        panel.transform.SetParent(canvas.transform, false);
        Image pImg = panel.AddComponent<Image>();
        pImg.sprite = Spr("UI/Panel.png");
        SetRect(pImg.rectTransform, Vector2.zero, new Vector2(1300, 780));

        MakeText(panel.transform, "Heading", "HOW TO PLAY", 70, Gold, new Vector2(0, 280), new Vector2(1000, 90));
        MakeText(panel.transform, "Body",
            "ARROW KEYS / WASD  -  Move the spaceship\n" +
            "SPACE  -  Shoot lasers\n\n" +
            "Collect STARS: +10 points\n" +
            "Shoot ASTEROIDS: +5 points\n" +
            "Crash into an asteroid: -10 points and GAME OVER",
            44, Color.white, new Vector2(0, 40), new Vector2(1100, 400));
        Button close = MakeButton(panel.transform, "CloseButton", "CLOSE", new Vector2(0, -270), 0.8f);

        menu.instructionsPanel = panel;
        UnityEventTools.AddPersistentListener(play.onClick, new UnityAction(menu.Play));
        UnityEventTools.AddPersistentListener(info.onClick, new UnityAction(menu.ShowInstructions));
        UnityEventTools.AddPersistentListener(quit.onClick, new UnityAction(menu.Quit));
        UnityEventTools.AddPersistentListener(close.onClick, new UnityAction(menu.HideInstructions));
        panel.SetActive(false);

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SceneDir + "MainMenu.unity");
    }

    static void BuildGameplay(GameObject laser, GameObject explosion, GameObject asteroid, GameObject star)
    {
        NewScene();
        Canvas canvas = MakeCanvas();

        Text score = MakeText(canvas.transform, "ScoreText", "SCORE: 0", 60, Gold, new Vector2(96, -80), new Vector2(600, 80));
        TopLeft(score.rectTransform);
        score.alignment = TextAnchor.MiddleLeft;

        Text hint = MakeText(canvas.transform, "HintText", "Arrows: move   Space: shoot", 32, Cyan, new Vector2(-64, -80), new Vector2(700, 60));
        TopRight(hint.rectTransform);
        hint.alignment = TextAnchor.MiddleRight;

        GameManager gm = new GameObject("GameManager").AddComponent<GameManager>();
        gm.scoreText = score;

        // Player
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, -3f, 0);
        SpriteRenderer psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = Spr("Sprites/Spaceship.png");
        psr.sortingOrder = 10;
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.38f;

        GameObject flame = new GameObject("EngineFlame");
        flame.transform.SetParent(player.transform, false);
        flame.transform.localPosition = new Vector3(0, -0.95f, 0);
        SpriteRenderer fsr = flame.AddComponent<SpriteRenderer>();
        fsr.sprite = Spr("Sprites/Engine_Flame.png");
        fsr.sortingOrder = 9;
        flame.AddComponent<EngineFlame>();

        GameObject fire = new GameObject("FirePoint");
        fire.transform.SetParent(player.transform, false);
        fire.transform.localPosition = new Vector3(0, 0.9f, 0);

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.laserPrefab = laser;
        pc.explosionPrefab = explosion;
        pc.firePoint = fire.transform;

        // Spawners
        Spawner asteroidSpawner = new GameObject("AsteroidSpawner").AddComponent<Spawner>();
        asteroidSpawner.kind = Spawner.Kind.Asteroid;
        asteroidSpawner.prefab = asteroid;
        asteroidSpawner.player = player.transform;
        asteroidSpawner.startCount = 5;
        asteroidSpawner.maxCount = 12;
        asteroidSpawner.rampEverySeconds = 15f;
        asteroidSpawner.spawnInterval = 1.2f;

        Spawner starSpawner = new GameObject("StarSpawner").AddComponent<Spawner>();
        starSpawner.kind = Spawner.Kind.Star;
        starSpawner.prefab = star;
        starSpawner.player = player.transform;
        starSpawner.startCount = 4;
        starSpawner.maxCount = 6;
        starSpawner.rampEverySeconds = 0f;
        starSpawner.spawnInterval = 2f;
        starSpawner.minDistanceFromPlayer = 1.5f;

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SceneDir + "Gameplay.unity");
    }

    static void BuildEndGame()
    {
        NewScene();
        Canvas canvas = MakeCanvas();
        MakeEventSystem();

        EndGameUI ui = new GameObject("EndGameUI").AddComponent<EndGameUI>();

        GameObject panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(canvas.transform, false);
        Image pImg = panel.AddComponent<Image>();
        pImg.sprite = Spr("UI/Panel.png");
        SetRect(pImg.rectTransform, Vector2.zero, new Vector2(1400, 820));
        // Fit the whole centered panel even in narrow or short Game views.
        canvas.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        MakeText(panel.transform, "Title", "GAME OVER", 110, new Color(1f, 0.4f, 0.3f), new Vector2(0, 200), new Vector2(1100, 140));
        ui.scoreText = MakeText(panel.transform, "ScoreText", "YOUR SCORE: 0", 70, Gold, new Vector2(0, 70), new Vector2(1100, 100));
        ui.highScoreText = MakeText(panel.transform, "HighScoreText", "HIGH SCORE: 0", 50, Cyan, new Vector2(0, -35), new Vector2(1100, 80));

        Button again = MakeButton(panel.transform, "PlayAgainButton", "PLAY AGAIN", new Vector2(-420, -190), 0.8f);
        Button menu = MakeButton(panel.transform, "MainMenuButton", "MAIN MENU", new Vector2(0, -190), 0.8f);
        Button quit = MakeButton(panel.transform, "QuitButton", "QUIT", new Vector2(420, -190), 0.8f);

        UnityEventTools.AddPersistentListener(again.onClick, new UnityAction(ui.PlayAgain));
        UnityEventTools.AddPersistentListener(menu.onClick, new UnityAction(ui.MainMenu));
        UnityEventTools.AddPersistentListener(quit.onClick, new UnityAction(ui.Quit));

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SceneDir + "EndGame.unity");
    }

    // ------------------------------------------------------------------ UI helpers


    static Canvas MakeCanvas()
    {
        GameObject go = new GameObject("Canvas");
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void MakeEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static void SetRect(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void TopLeft(RectTransform rt)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(96, -80);
    }

    static void TopRight(RectTransform rt)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = new Vector2(-64, -80);
    }

    static Font GetFont()
    {
#if UNITY_2022_2_OR_NEWER
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
    }

    static Text MakeText(Transform parent, string name, string content, int size, Color color, Vector2 pos, Vector2 boxSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = GetFont();
        t.text = content;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        SetRect(t.rectTransform, pos, boxSize);

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(3, -3);
        return t;
    }

    /// <summary>Nút bấm dùng 3 sprite: Normal / Highlight (hover) / Dark (nhấn).</summary>
    static Button MakeButton(Transform parent, string name, string label, Vector2 pos, float scale = 1f)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.sprite = Spr("UI/Button_Normal.png");
        SetRect(img.rectTransform, pos, new Vector2(460, 220) * scale);

        Button b = go.AddComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.SpriteSwap;
        SpriteState ss = new SpriteState
        {
            highlightedSprite = Spr("UI/Button_Highlight.png"),
            selectedSprite = Spr("UI/Button_Highlight.png"),
            pressedSprite = Spr("UI/Button_Dark.png"),
        };
        b.spriteState = ss;

        Text t = MakeText(go.transform, "Label", label, Mathf.RoundToInt(48 * scale), new Color(0.02f, 0.1f, 0.2f),
            Vector2.zero, new Vector2(400, 120) * scale);
        t.GetComponent<Outline>().enabled = false;
        return b;
    }
}
