using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Chuyển scene mượt: màn hình mờ dần sang đen, load scene, rồi sáng dần lên.
/// Object này tự tạo bằng code và tồn tại xuyên suốt các scene (DontDestroyOnLoad),
/// nên không cần đặt sẵn trong scene nào. Dùng: SceneFader.Load("Gameplay");
/// </summary>
public class SceneFader : MonoBehaviour
{
    const float FadeTime = 0.5f;

    static SceneFader instance;
    CanvasGroup group;
    bool loading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        Ensure();
        instance.StartCoroutine(instance.Fade(1f, 0f)); // fade-in scene đầu tiên
    }

    static void Ensure()
    {
        if (instance != null) return;

        GameObject go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneFader>();

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        instance.group = go.AddComponent<CanvasGroup>();
        instance.group.blocksRaycasts = false;

        GameObject img = new GameObject("Black", typeof(RectTransform));
        img.transform.SetParent(go.transform, false);
        Image image = img.AddComponent<Image>();
        image.color = Color.black;
        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            instance.StartCoroutine(instance.Fade(1f, 0f));
        };
    }

    /// <summary>Load scene theo tên với hiệu ứng fade.</summary>
    public static void Load(string sceneName)
    {
        Ensure();
        if (instance.loading) return;
        instance.StartCoroutine(instance.LoadRoutine(sceneName));
    }

    /// <summary>Thoát game (trong Editor thì dừng Play Mode).</summary>
    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        loading = true;
        group.blocksRaycasts = true; // chặn click trong lúc chuyển cảnh
        yield return Fade(group.alpha, 1f);
        SceneManager.LoadScene(sceneName);
        loading = false;
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < FadeTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / FadeTime);
            yield return null;
        }
        group.alpha = to;
        group.blocksRaycasts = to > 0.5f;
    }
}
