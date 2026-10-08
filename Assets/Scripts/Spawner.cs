using UnityEngine;

/// <summary>
/// Sinh thiên thạch hoặc sao ngẫu nhiên trong màn hình, luôn giữ đủ số lượng tối đa.
/// Thiên thạch tăng dần số lượng theo thời gian để game khó hơn.
/// </summary>
public class Spawner : MonoBehaviour
{
    public enum Kind { Asteroid, Star }

    public Kind kind = Kind.Asteroid;
    public GameObject prefab;

    [Header("Số lượng")]
    public int startCount = 5;
    public int maxCount = 12;
    [Tooltip("Cứ sau mỗi N giây thì tăng giới hạn thêm 1 (0 = không tăng)")]
    public float rampEverySeconds = 15f;

    [Header("Tốc độ sinh")]
    public float spawnInterval = 1.2f;

    [Tooltip("Không sinh gần tàu hơn khoảng này")]
    public float minDistanceFromPlayer = 3f;
    public Transform player;

    float nextSpawnTime;
    float startTime;

    void Start()
    {
        startTime = Time.time;

        // Sinh sẵn một lượt ban đầu cho màn hình không bị trống
        for (int i = 0; i < startCount; i++) Spawn();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (Time.time < nextSpawnTime) return;

        nextSpawnTime = Time.time + spawnInterval;
        if (CurrentCount() < CurrentLimit()) Spawn();
    }

    int CurrentCount()
    {
        return kind == Kind.Asteroid ? Asteroid.Count : Star.Count;
    }

    int CurrentLimit()
    {
        if (rampEverySeconds <= 0f) return maxCount;
        int extra = Mathf.FloorToInt((Time.time - startTime) / rampEverySeconds);
        return Mathf.Min(maxCount, startCount + extra);
    }

    void Spawn()
    {
        Vector2 pos = Vector2.zero;
        // Thử vài lần để tìm vị trí đủ xa tàu
        for (int i = 0; i < 10; i++)
        {
            pos = ScreenBounds.RandomInside(0.6f);
            if (player == null || !player.gameObject.activeInHierarchy ||
                Vector2.Distance(pos, player.position) >= minDistanceFromPlayer) break;
        }
        Instantiate(prefab, pos, Quaternion.identity);
    }
}
