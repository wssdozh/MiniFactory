using UnityEditor;
using UnityEngine;

namespace MiniFactory.Editor
{
    public sealed class MiniFactoryArtPostprocessor : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/_Project/Art";
        private const string TilesRoot = "Assets/_Project/Art/Tiles";

        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith(ArtRoot) == false)
                return;

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            if (assetPath.StartsWith(TilesRoot))
                importer.spritePixelsPerUnit = 16;
        }
    }
}
