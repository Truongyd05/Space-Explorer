using UnityEngine;

/// <summary>
/// Tiện ích tính vùng nhìn thấy của camera orthographic (camera đặt tại gốc toạ độ).
/// </summary>
public static class ScreenBounds
{
    /// <summary>Nửa chiều rộng/cao của màn hình (đơn vị world), trừ đi margin.</summary>
    public static Vector2 Half(float margin = 0f)
    {
        Camera cam = Camera.main;
        float h = cam.orthographicSize;
        return new Vector2(h * cam.aspect - margin, h - margin);
    }

    /// <summary>Vị trí ngẫu nhiên nằm trong màn hình.</summary>
    public static Vector2 RandomInside(float margin)
    {
        Vector2 half = Half(margin);
        return new Vector2(Random.Range(-half.x, half.x), Random.Range(-half.y, half.y));
    }
}
