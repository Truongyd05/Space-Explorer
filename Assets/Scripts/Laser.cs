using UnityEngine;

/// <summary>
/// Tia laser bay thẳng lên. Phá huỷ thiên thạch khi chạm và tự xoá khi ra khỏi màn hình.
/// Yêu cầu: Rigidbody2D (Dynamic, gravity = 0) + Collider2D (isTrigger).
/// </summary>
public class Laser : MonoBehaviour
{
    public float speed = 12f;

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (transform.position.y > ScreenBounds.Half().y + 1f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Asteroid asteroid = other.GetComponent<Asteroid>();
        if (asteroid != null)
        {
            asteroid.Hit();
            Destroy(gameObject);
        }
    }
}
