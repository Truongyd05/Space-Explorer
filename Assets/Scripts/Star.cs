using UnityEngine;

/// <summary>
/// Ngôi sao có thể nhặt: nhấp nháy nhẹ, tự biến mất sau một thời gian.
/// Yêu cầu: Rigidbody2D (Kinematic) + Collider2D (isTrigger).
/// </summary>
public class Star : MonoBehaviour
{
    /// <summary>Số sao đang tồn tại (spawner dùng để giữ số lượng).</summary>
    public static int Count;

    public float lifetime = 12f;
    public float pulseSpeed = 4f;
    public float pulseAmount = 0.1f;
    public GameObject sparklePrefab;

    float age;
    Vector3 baseScale;

    void OnEnable()  { Count++; }
    void OnDisable() { Count--; }

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // Hiệu ứng phập phồng; nhấp nháy nhanh hơn ở 3 giây cuối
        float speed = (lifetime - age < 3f) ? pulseSpeed * 3f : pulseSpeed;
        float s = 1f + Mathf.Sin(Time.time * speed) * pulseAmount;
        transform.localScale = baseScale * s;
    }

    /// <summary>Được người chơi nhặt: cộng điểm và hiện hiệu ứng lấp lánh.</summary>
    public void Collect()
    {
        GameAudio.Collect();
        if (sparklePrefab != null)
            Instantiate(sparklePrefab, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(GameManager.Instance.starPoints);

        Destroy(gameObject);
    }
}
