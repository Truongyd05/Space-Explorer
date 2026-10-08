using UnityEngine;

/// <summary>
/// Điều khiển tàu vũ trụ: di chuyển 8 hướng bằng phím mũi tên (hoặc WASD),
/// bắn laser bằng phím Space, xử lý va chạm với thiên thạch và sao.
/// Yêu cầu: Rigidbody2D (Dynamic, gravity = 0) + Collider2D (isTrigger).
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    [Tooltip("Khoảng cách tối thiểu tới mép màn hình")]
    public float padding = 0.7f;

    [Header("Shooting")]
    public GameObject laserPrefab;
    public Transform firePoint;       // vị trí mũi tàu
    public float fireCooldown = 0.25f;

    [Header("Effects")]
    public GameObject explosionPrefab;

    float nextFireTime;
    bool dead;

    void Update()
    {
        if (dead || Time.timeScale == 0f) return;

        Move();

        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    /// <summary>Di chuyển theo Transform (Position) và giữ tàu trong màn hình.</summary>
    void Move()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize(); // đi chéo không nhanh hơn

        Vector3 pos = transform.position + (Vector3)(input * moveSpeed * Time.deltaTime);

        Vector2 half = ScreenBounds.Half(padding);
        pos.x = Mathf.Clamp(pos.x, -half.x, half.x);
        pos.y = Mathf.Clamp(pos.y, -half.y, half.y);
        transform.position = pos;
    }

    void Shoot()
    {
        nextFireTime = Time.time + fireCooldown;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Instantiate(laserPrefab, spawnPos, Quaternion.identity);
        GameAudio.Shoot();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (dead || Time.timeScale == 0f) return;

        Star star = other.GetComponent<Star>();
        if (star != null)
        {
            star.Collect();
            return;
        }

        if (other.GetComponent<Asteroid>() != null)
        {
            Die();
        }
    }

    /// <summary>Va chạm thiên thạch: trừ điểm, nổ tung và kết thúc game.</summary>
    void Die()
    {
        dead = true;
        GameAudio.Explode();
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        GameManager gm = GameManager.Instance;
        gm.AddScore(-gm.asteroidHitPenalty);
        gm.GameOver();

        gameObject.SetActive(false);
    }
}
