using UnityEngine;

/// <summary>
/// Thiên thạch trôi ngẫu nhiên: hướng/tốc độ/kích thước/sprite đều random,
/// thỉnh thoảng đổi hướng và nảy lại khi chạm mép màn hình.
/// Yêu cầu: Rigidbody2D (Kinematic) + Collider2D (isTrigger).
/// </summary>
public class Asteroid : MonoBehaviour
{
    /// <summary>Số thiên thạch đang tồn tại (spawner dùng để giữ số lượng).</summary>
    public static int Count;

    [Header("Look")]
    public Sprite[] sprites;              // 3 loại: xanh, nâu, xám
    public Vector2 scaleRange = new Vector2(0.7f, 1.4f);

    [Header("Movement")]
    public Vector2 speedRange = new Vector2(1.5f, 3.5f);
    public float maxSpinSpeed = 60f;       // độ/giây
    public Vector2 turnIntervalRange = new Vector2(1.5f, 4f);
    public float maxTurnAngle = 45f;

    [Header("Effects")]
    public GameObject explosionPrefab;

    Vector2 direction;
    float speed;
    float spin;
    float nextTurnTime;

    void OnEnable()  { Count++; }
    void OnDisable() { Count--; }

    void Start()
    {
        if (sprites != null && sprites.Length > 0)
            GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];

        transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);

        direction = Random.insideUnitCircle.normalized;
        speed = Random.Range(speedRange.x, speedRange.y);
        spin = Random.Range(-maxSpinSpeed, maxSpinSpeed);
        ScheduleTurn();
    }

    void Update()
    {
        // Đổi hướng ngẫu nhiên theo chu kỳ -> chuyển động "ngẫu nhiên"
        if (Time.time >= nextTurnTime)
        {
            direction = Quaternion.Euler(0, 0, Random.Range(-maxTurnAngle, maxTurnAngle)) * direction;
            ScheduleTurn();
        }

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        transform.Rotate(0, 0, spin * Time.deltaTime);

        // Nảy lại khi chạm mép màn hình
        Vector2 half = ScreenBounds.Half(0.3f);
        Vector3 p = transform.position;
        if ((p.x > half.x && direction.x > 0) || (p.x < -half.x && direction.x < 0)) direction.x = -direction.x;
        if ((p.y > half.y && direction.y > 0) || (p.y < -half.y && direction.y < 0)) direction.y = -direction.y;
    }

    void ScheduleTurn()
    {
        nextTurnTime = Time.time + Random.Range(turnIntervalRange.x, turnIntervalRange.y);
    }

    /// <summary>Bị laser bắn trúng: nổ tung và cộng điểm.</summary>
    public void Hit()
    {
        GameAudio.Explode();
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(GameManager.Instance.asteroidShotPoints);

        Destroy(gameObject);
    }
}
