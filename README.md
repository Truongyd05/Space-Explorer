# Space Explorer – Unity 2D (Lab 1)

Điều khiển tàu vũ trụ, né/bắn thiên thạch và nhặt sao để lấy điểm. Dùng bộ asset pixel art trong `Lab1Assets_Pixel`.

## Cách mở và chạy
1. Cài **Unity 2022.3 LTS** trở lên (Unity 6 cũng được), dùng **Input Manager (Old)** – mặc định khi tạo project mới bằng folder.
2. Unity Hub → *Add project from disk* → chọn thư mục `SpaceExplorer`.
3. Lần đầu mở, script `Assets/Editor/SpaceExplorerBuilder.cs` **tự dựng** prefab, 3 scene và Build Settings
   (nếu chưa thấy, chọn menu **Space Explorer → Build Game**).
4. Mở `Assets/Scenes/MainMenu.unity` và bấm **Play**.

## Điều khiển
| Phím | Chức năng |
|---|---|
| Mũi tên / WASD | Di chuyển 8 hướng |
| Space (giữ) | Bắn laser |
| M | Bật/tắt toàn bộ âm thanh (lưu lựa chọn) |
| MENU (giữa phía trên) / Esc | Tạm dừng hoặc tiếp tục |

## Luật chơi & điểm
| Sự kiện | Điểm |
|---|---|
| Nhặt sao | +10 |
| Bắn nổ thiên thạch | +5 |
| Tàu đâm thiên thạch | −10 và **Game Over** |

## Các scene (Assets/Scenes)
| Scene | Nội dung |
|---|---|
| **MainMenu** | Tiêu đề, nút **Play** (→ Gameplay), **Instructions** (mở panel hướng dẫn), **Quit**. |
| **Gameplay** | Tàu, thiên thạch, sao, HUD điểm. Kết thúc khi tàu đâm thiên thạch. |
| **EndGame** | Hiện điểm + điểm cao nhất; nút **Play Again**, **Main Menu**, **Quit**. |

Chuyển scene đều qua `SceneFader` (mờ dần sang đen rồi sáng lại).

## Các thành phần game
| Đối tượng | Script | Component Unity | Mô tả |
|---|---|---|---|
| Spaceship (Player) | `PlayerController` | SpriteRenderer, Rigidbody2D (Dynamic, Gravity 0), CircleCollider2D (Trigger) | Di chuyển bằng `Transform.position`, bị giới hạn trong màn hình; bắn `Laser` từ `FirePoint`; xử lý va chạm sao / thiên thạch. Con `EngineFlame` nhấp nháy (`EngineFlame`). |
| Laser | `Laser` | Rigidbody2D (Dynamic), CapsuleCollider2D (Trigger) | Bay lên; trúng thiên thạch thì phá nó và tự huỷ; ra khỏi màn hình cũng tự huỷ. |
| Asteroid | `Asteroid` | Rigidbody2D (Kinematic), CircleCollider2D (Trigger) | Random sprite (3 loại), kích thước, tốc độ, hướng; xoay; đổi hướng ngẫu nhiên mỗi 1.5–4 giây; nảy lại ở mép màn hình. |
| Star | `Star` | Rigidbody2D (Kinematic), CircleCollider2D (Trigger) | Phập phồng; tự biến mất sau 12 giây (nhấp nháy nhanh ở 3 giây cuối). |
| Spawner | `Spawner` | – | Sinh thiên thạch/sao ngẫu nhiên, giữ đủ số lượng, không sinh sát tàu; số thiên thạch tăng dần mỗi 15 giây (5 → 12). |
| Hiệu ứng nổ / lấp lánh | `AnimatedEffect` | SpriteRenderer | Chạy 4 frame `Explosion_01..04` hoặc `Collect_Sparkle` rồi tự huỷ. |
| GameManager | `GameManager` | – | Giữ điểm, cập nhật HUD, xử lý Game Over, lưu điểm cao bằng `PlayerPrefs`. |

### Logic va chạm
Mọi collider là **Trigger**, nên các va chạm được xử lý trong `OnTriggerEnter2D` và tàu không bị đẩy đi.
Để trigger hoạt động, mỗi cặp va chạm có ít nhất một Rigidbody2D Dynamic (tàu, laser) – thiên thạch và sao là Kinematic.

- Tàu ↔ Star → `Star.Collect()` → +10, hiệu ứng lấp lánh.
- Tàu ↔ Asteroid → `PlayerController.Die()` → −10, nổ, chờ 1.5s rồi sang EndGame.
- Laser ↔ Asteroid → `Asteroid.Hit()` → +5, nổ.

## Cấu trúc thư mục
```
Assets/
  Art/        sprite, UI, background (từ Lab1Assets_Pixel)
  Editor/     SpriteImportSettings (cấu hình import pixel art), SpaceExplorerBuilder (dựng scene/prefab)
  Prefabs/    Laser, Asteroid, Star, Explosion, CollectSparkle (tạo tự động)
  Scenes/     MainMenu, Gameplay, EndGame (tạo tự động)
  Scripts/    toàn bộ script gameplay
```

## Ghi chú
- Ảnh gốc rất lớn (~1200 px), nên `SpriteImportSettings` đặt *Pixels Per Unit* riêng cho từng sprite để kích thước trong game hợp lý, và dùng Filter Mode **Point** để giữ nét pixel.
- Camera orthographic size 5; mọi giới hạn màn hình tính từ camera (`ScreenBounds`), nên đổi tỉ lệ màn hình vẫn đúng.

## Âm thanh và Game Over
- Nhạc nền điện tử lặp ở menu và khi chơi; tiếng laser, nhặt sao, nổ, bấm nút và giai điệu Game Over. Các WAV được tổng hợp riêng cho dự án trong `Assets/Resources/Audio`.
- `GameAudio` tự khởi tạo khi Play, tồn tại qua các scene và bổ sung AudioListener cho camera nếu thiếu; không cần gán clip thủ công.
- EndGame gom điểm và ba nút vào panel căn giữa. Canvas dùng Expand để giữ toàn bộ panel trong màn hình ở các tỉ lệ khác nhau; builder cũng đã cập nhật cùng bố cục.

Menu tạm dừng gồm **RESUME**, **MAIN MENU** (rời ván hiện tại) và **QUIT GAME**. Khi mở menu, thời gian game và thao tác bắn/va chạm của tàu được dừng; khi tiếp tục hoặc rời scene, tốc độ thời gian được khôi phục. `PauseMenuUI` được GameManager tự tạo, dùng được ngay với Gameplay hiện có.
