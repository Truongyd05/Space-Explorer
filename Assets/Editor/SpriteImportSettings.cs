using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tự cấu hình import cho mọi ảnh trong Assets/Art:
/// - Texture Type = Sprite, Filter = Point (giữ nét pixel art), không nén.
/// - Pixels Per Unit được tính để mỗi sprite có kích thước world hợp lý
///   (ảnh gốc rất lớn ~1200px nên không thể dùng PPU mặc định 100).
/// </summary>
public class SpriteImportSettings : AssetPostprocessor
{
    // tên file -> (cạnh lớn nhất tính bằng pixel, kích thước mong muốn theo đơn vị world)
    static readonly Dictionary<string, Vector2> Sizes = new Dictionary<string, Vector2>
    {
        { "Spaceship",       new Vector2(1365, 1.5f) },
        { "Asteroid_Blue",   new Vector2(1254, 1.2f) },
        { "Asteroid_Brown",  new Vector2(1270, 1.2f) },
        { "Asteroid_Gray",   new Vector2(1254, 1.2f) },
        { "Star",            new Vector2(1287, 0.7f) },
        { "Laser",           new Vector2(1978, 0.9f) },
        { "Engine_Flame",    new Vector2(1681, 0.9f) },
        { "Collect_Sparkle", new Vector2(1283, 0.9f) },
        { "Explosion_01",    new Vector2(1254, 1.8f) },
        { "Explosion_02",    new Vector2(1254, 1.8f) },
        { "Explosion_03",    new Vector2(1254, 1.8f) },
        { "Explosion_04",    new Vector2(1254, 1.8f) },
        { "Space_Nebula",    new Vector2(1536, 20f) }, // rộng 20 unit
    };

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Art/")) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = 2048;

        string name = Path.GetFileNameWithoutExtension(assetPath);
        importer.spritePixelsPerUnit = Sizes.TryGetValue(name, out Vector2 s) ? s.x / s.y : 100f;
    }
}
