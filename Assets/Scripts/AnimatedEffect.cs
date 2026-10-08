using UnityEngine;

/// <summary>
/// Hiệu ứng một lần (nổ, lấp lánh): chạy lần lượt các frame, phóng to dần,
/// có thể mờ dần, rồi tự huỷ.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedEffect : MonoBehaviour
{
    public Sprite[] frames;
    public float duration = 0.5f;
    public float startScale = 0.6f;
    public float endScale = 1f;
    public bool fadeOut;

    SpriteRenderer sr;
    float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);

        if (frames != null && frames.Length > 0)
            sr.sprite = frames[Mathf.Min(frames.Length - 1, (int)(k * frames.Length))];

        transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, k);

        if (fadeOut)
        {
            Color c = sr.color;
            c.a = 1f - k;
            sr.color = c;
        }

        if (k >= 1f) Destroy(gameObject);
    }
}
