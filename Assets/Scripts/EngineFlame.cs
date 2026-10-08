using UnityEngine;

/// <summary>Làm ngọn lửa động cơ nhấp nháy bằng cách đổi kích thước theo thời gian.</summary>
public class EngineFlame : MonoBehaviour
{
    public float speed = 25f;
    public float amount = 0.2f;

    Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float flicker = 1f + Mathf.Sin(Time.time * speed) * amount + Random.Range(-0.05f, 0.05f);
        transform.localScale = new Vector3(baseScale.x, baseScale.y * flicker, baseScale.z);
    }
}
