using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Gameplay menu: freezes simulation and input, with unscaled UI controls.</summary>
[DefaultExecutionOrder(-100)]
public class PauseMenuUI : MonoBehaviour
{
    public bool IsPaused { get; private set; }
    GameObject overlay;
    GameObject canvasObject;
    Button menuButton;
    Button resumeButton;
    float previousTimeScale = 1f;
    bool leaving;
    readonly Color cyan = new Color(0.4f, 0.9f, 1f);

    void Awake()
    {
        canvasObject = new GameObject("PauseCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        // Match the gameplay HUD scale so all three header items share a row.
        scaler.matchWidthOrHeight = 0.5f;
        if (EventSystem.current == null)
        {
            var events = new GameObject("GameplayEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            events.transform.SetParent(transform, false);
        }

        menuButton = MakeMenuButton(canvas.transform);
        var menuRect = menuButton.GetComponent<RectTransform>();
        menuRect.anchorMin = menuRect.anchorMax = new Vector2(.5f, 1f);
        menuRect.pivot = new Vector2(.5f, .5f);
        menuRect.anchoredPosition = new Vector2(0, -80);

        overlay = MakeRect(canvas.transform, "PauseOverlay", Vector2.zero, Vector2.zero);
        var overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
        overlay.AddComponent<Image>().color = new Color(0, 0, .04f, .78f);

        var panel = MakeRect(overlay.transform, "PausePanel", Vector2.zero, new Vector2(920, 740));
        panel.AddComponent<Image>().color = new Color(.025f, .06f, .16f, .98f);
        var border = panel.AddComponent<Outline>();
        border.effectColor = cyan;
        border.effectDistance = new Vector2(3, -3);
        MakeText(panel.transform, "Title", "PAUSED", new Vector2(0, 245), new Vector2(800, 100), 70, cyan);
        MakeText(panel.transform, "Hint", "ESC / MENU - Resume    M - Sound", new Vector2(0, 150), new Vector2(800, 60), 30, Color.white);
        resumeButton = MakeButton(panel.transform, "ResumeButton", "RESUME", new Vector2(0, 40), new Vector2(540, 100), Resume);
        MakeButton(panel.transform, "MainMenuButton", "MAIN MENU", new Vector2(0, -90), new Vector2(540, 100), MainMenu);
        MakeButton(panel.transform, "QuitButton", "QUIT GAME", new Vector2(0, -220), new Vector2(540, 100), Quit);
        overlay.SetActive(false);
        menuButton.transform.SetAsLastSibling();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        bool canPause = gm != null && !gm.IsGameOver && !leaving;
        menuButton.interactable = canPause;
        if (canPause && Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    public void TogglePause()
    {
        if (leaving || GameManager.Instance == null || GameManager.Instance.IsGameOver) return;
        if (IsPaused) { Resume(); return; }
        GameAudio.Click();
        previousTimeScale = Time.timeScale;
        IsPaused = true;
        Time.timeScale = 0f;
        overlay.SetActive(true);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void Resume()
    {
        if (!IsPaused || leaving) return;
        GameAudio.Click();
        RestoreTime();
        overlay.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void MainMenu()
    {
        if (leaving) return;
        leaving = true;
        GameAudio.Click();
        // SceneFader uses unscaled time; keep gameplay frozen until it unloads.
        SceneFader.Load("MainMenu");
    }

    public void Quit()
    {
        if (leaving) return;
        leaving = true;
        GameAudio.Click();
        RestoreTime();
        SceneFader.Quit();
    }

    void RestoreTime()
    {
        if (!IsPaused) return;
        Time.timeScale = previousTimeScale;
        IsPaused = false;
    }

    void OnDisable()
    {
        RestoreTime();
        if (overlay != null) overlay.SetActive(false);
    }

    static GameObject MakeRect(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return go;
    }

    static Text MakeText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, Color color)
    {
        var go = MakeRect(parent, name, position, size);
        var text = go.AddComponent<Text>();
#if UNITY_2022_2_OR_NEWER
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
    }

    Button MakeMenuButton(Transform parent)
    {
        var go = MakeRect(parent, "MenuButton", Vector2.zero, new Vector2(64, 64));
        var background = go.AddComponent<Image>();
        background.color = new Color(.025f, .06f, .16f, .8f);
        // Four crisp edges stay visible over both dark space and bright nebulae.
        for (int i = 0; i < 4; i++)
        {
            bool horizontal = i < 2;
            float edge = (i % 2 == 0) ? 30.5f : -30.5f;
            var border = MakeRect(go.transform, "Border" + i,
                horizontal ? new Vector2(0, edge) : new Vector2(edge, 0),
                horizontal ? new Vector2(64, 3) : new Vector2(3, 64));
            var edgeImage = border.AddComponent<Image>();
            edgeImage.color = cyan;
            edgeImage.raycastTarget = false;
        }
        var button = go.AddComponent<Button>();
        button.targetGraphic = background;
        var colors = button.colors;
        colors.highlightedColor = new Color(1.4f, 1.8f, 2f);
        colors.pressedColor = new Color(.6f, .7f, .8f);
        button.colors = colors;
        button.onClick.AddListener(TogglePause);
        for (int i = 0; i < 3; i++)
        {
            var bar = MakeRect(go.transform, "Bar" + i, new Vector2(0, 10 - i * 10), new Vector2(32, 4));
            var image = bar.AddComponent<Image>();
            image.color = cyan;
            image.raycastTarget = false;
        }
        return button;
    }

    Button MakeButton(Transform parent, string name, string label, Vector2 position, Vector2 size, UnityAction action)
    {
        var go = MakeRect(parent, name, position, size);
        var image = go.AddComponent<Image>();
        image.color = cyan;
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(.75f, 1f, 1f);
        colors.pressedColor = new Color(.45f, .65f, .75f);
        button.colors = colors;
        button.onClick.AddListener(action);
        MakeText(go.transform, "Label", label, Vector2.zero, size - new Vector2(20, 10), 38, new Color(.02f, .08f, .16f));
        return button;
    }
}
