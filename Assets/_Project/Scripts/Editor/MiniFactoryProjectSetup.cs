using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MiniFactory.Editor
{
    public static class MiniFactoryProjectSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/Main.unity";
        private const string TilesFolder = "Assets/_Project/Art/Tiles/TinyFactory/Tiles";
        private const string TileAssetsFolder = "Assets/_Project/Art/Tiles/TinyFactory/TileAssets";
        private const string PalettesFolder = "Assets/_Project/Art/Tiles/TinyFactory/Palettes";
        private const string PalettePrefabPath = "Assets/_Project/Art/Tiles/TinyFactory/Palettes/TinyFactoryPalette.prefab";

        [MenuItem("Mini Factory/Run Full Project Setup")]
        public static void RunFullSetup()
        {
            Debug.Log("[MiniFactory] Starting project setup...");

            ConfigurePlayerSettings();
            ReimportKenneyArt();
            EnsureFolderExists(TileAssetsFolder);
            EnsureFolderExists(PalettesFolder);
            CreateTileAssets();
            CreateTilePalettePrefab();
            CreateMainScene();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MiniFactory] Setup completed successfully!");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "MiniFactory";
            PlayerSettings.productName = "Mini Factory";

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.minifactory.game");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.minifactory.game");

            Debug.Log("[MiniFactory] Player Settings configured (Portrait, Android package: com.minifactory.game).");
        }

        private static void ReimportKenneyArt()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new string[] { "Assets/_Project/Art" });
            int reimportedCount = 0;

            for (int i = 0; i < textureGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;

                    if (path.Contains("/Tiles/"))
                    {
                        importer.spritePixelsPerUnit = 16;
                    }

                    importer.SaveAndReimport();
                    reimportedCount++;
                }
            }

            Debug.Log($"[MiniFactory] Reimported {reimportedCount} textures with Point filter & PPU settings.");
        }

        private static void CreateTileAssets()
        {
            string[] tileGuids = AssetDatabase.FindAssets("t:Sprite", new string[] { TilesFolder });
            int createdCount = 0;

            for (int i = 0; i < tileGuids.Length; i++)
            {
                string spritePath = AssetDatabase.GUIDToAssetPath(tileGuids[i]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                if (sprite == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(spritePath);
                string tileAssetPath = $"{TileAssetsFolder}/{assetName}.asset";

                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tileAssetPath);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    tile.color = Color.white;
                    AssetDatabase.CreateAsset(tile, tileAssetPath);
                    createdCount++;
                }
                else
                {
                    tile.sprite = sprite;
                    EditorUtility.SetDirty(tile);
                }
            }

            Debug.Log($"[MiniFactory] Created/updated {createdCount} Tile assets in {TileAssetsFolder}.");
        }

        private static void CreateTilePalettePrefab()
        {
            GameObject paletteRoot = new GameObject("TinyFactoryPalette");
            Grid grid = paletteRoot.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            GameObject layer = new GameObject("Layer1");
            layer.transform.SetParent(paletteRoot.transform);
            Tilemap tilemap = layer.AddComponent<Tilemap>();
            layer.AddComponent<TilemapRenderer>();

            string[] tileGuids = AssetDatabase.FindAssets("t:Tile", new string[] { TileAssetsFolder });
            int columns = 12;

            for (int i = 0; i < tileGuids.Length; i++)
            {
                string tilePath = AssetDatabase.GUIDToAssetPath(tileGuids[i]);
                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);

                if (tile != null)
                {
                    int x = i % columns;
                    int y = -(i / columns);
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }

            GridPalette paletteSettings = ScriptableObject.CreateInstance<GridPalette>();
            paletteSettings.name = "Palette Settings";
            paletteSettings.cellSizing = GridPalette.CellSizing.Automatic;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(paletteRoot, PalettePrefabPath);
            AssetDatabase.AddObjectToAsset(paletteSettings, PalettePrefabPath);
            EditorUtility.SetDirty(prefab);

            Object.DestroyImmediate(paletteRoot);
            Debug.Log($"[MiniFactory] Created Tile Palette prefab at {PalettePrefabPath}.");
        }

        private static void CreateMainScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject cameraGo = new GameObject("Main Camera");
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.125f, 0.11f, 0.14f, 1f);
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            cameraGo.tag = "MainCamera";

            // 2. Global Light 2D for URP
            GameObject lightGo = new GameObject("Global Light 2D");
            Light2D light2d = lightGo.AddComponent<Light2D>();
            light2d.lightType = Light2D.LightType.Global;
            light2d.intensity = 1f;
            light2d.color = Color.white;

            // 3. Grid & Tilemap
            GameObject gridGo = new GameObject("Grid");
            Grid grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            GameObject floorTilemapGo = new GameObject("Tilemap_Floor");
            floorTilemapGo.transform.SetParent(gridGo.transform);
            Tilemap floorTilemap = floorTilemapGo.AddComponent<Tilemap>();
            TilemapRenderer floorRenderer = floorTilemapGo.AddComponent<TilemapRenderer>();
            floorRenderer.sortingOrder = 0;

            // 4. Paint starter floor area
            Tile floorTile = AssetDatabase.LoadAssetAtPath<Tile>($"{TileAssetsFolder}/tile_0001.asset");
            if (floorTile == null)
            {
                string[] anyTiles = AssetDatabase.FindAssets("t:Tile", new string[] { TileAssetsFolder });
                if (anyTiles.Length > 0)
                {
                    floorTile = AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(anyTiles[0]));
                }
            }

            if (floorTile != null)
            {
                int minX = -5;
                int maxX = 4;
                int minY = -8;
                int maxY = 7;

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                    }
                }
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[MiniFactory] Saved Main scene with Camera, 2D Light, and Tilemap floor at {ScenePath}.");
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
            bool alreadyAdded = false;

            for (int i = 0; i < currentScenes.Length; i++)
            {
                if (currentScenes[i].path == ScenePath)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (alreadyAdded == false)
            {
                EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
                for (int i = 0; i < currentScenes.Length; i++)
                {
                    newScenes[i] = currentScenes[i];
                }

                newScenes[currentScenes.Length] = new EditorBuildSettingsScene(ScenePath, true);
                EditorBuildSettings.scenes = newScenes;
                Debug.Log($"[MiniFactory] Added {ScenePath} to EditorBuildSettings.");
            }
        }

        private static void EnsureFolderExists(string path)
        {
            if (AssetDatabase.IsValidFolder(path) == false)
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
