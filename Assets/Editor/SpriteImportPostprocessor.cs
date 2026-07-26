using UnityEditor;

/// <summary>
/// 资源导入后处理器
/// 确保从原版游戏提取的 PNG 全部以 Sprite 类型导入，
/// 以便 UI Image / SpriteRenderer 正常引用。
/// </summary>
public class SpriteImportPostprocessor : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        // Sprites 目录 -> 导入为 Sprite (UI / 2D)
        if (assetPath.Contains("Resources/Sprites"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point; // 像素画使用点过滤，保持锐利
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
        // Textures 目录 -> 普通 2D 贴图 (可用作材质 albedo)
        else if (assetPath.Contains("Resources/Textures"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
        }
    }
}
