using System.Collections.Generic;
using System.IO;
using RapidPro.Core;
using RapidPro.GameLoop;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Makes the heart level from Level-Heart.png: every playable cell filled, always solvable.
/// Menu: YT-Playables > Levels > Generate Heart Level.
/// </summary>
public static class ArrowLevelGeneratorMenu
{
    private const string ShapePath = "Assets/Game/Levels/Textures/Level-Heart.png";
    private const string LevelPath = "Assets/Game/Levels/Data/Level_Heart.asset";
    private const string CatalogPath = "Assets/Game/Levels/Data/LevelCatalog.asset";
    private const int MinLength = 2;
    private const int MaxLength = 5;
    private const int SeedsToTry = 5000;

    [MenuItem("YT-Playables/Levels/Generate Heart Level")]
    public static void GenerateHeartLevel()
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ShapePath);
        if (texture == null)
        {
            Log.Error($"Level generator: no shape image at {ShapePath}.");
            return;
        }

        ShapeMask mask = ShapeLoader.FromTexture(texture);

        for (int seed = 1; seed <= SeedsToTry; seed++)
        {
            if (!ArrowLevelGenerator.TryGenerate(mask, seed, MinLength, MaxLength, out List<ArrowDefinition> arrows))
                continue;

            var level = LoadOrCreate<ArrowLevelData>(LevelPath);
            level.Set(mask.Width, mask.Height, ArrowLevelGenerator.GetRestrictedCells(mask), arrows, seed);
            EditorUtility.SetDirty(level);

            var catalog = LoadOrCreate<LevelCatalog>(CatalogPath);
            if (catalog.Count == 0)
            {
                catalog.Add(level);
                EditorUtility.SetDirty(catalog);
            }

            AssetDatabase.SaveAssets();
            Log.Info($"Level generator: made {LevelPath} with {arrows.Count} arrows (seed {seed}).");
            return;
        }

        Log.Error($"Level generator: no solvable level found in {SeedsToTry} seeds. Try other arrow lengths.");
    }

    private static T LoadOrCreate<T>(string p_path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(p_path);
        if (asset != null)
            return asset;

        Directory.CreateDirectory(Path.GetDirectoryName(p_path));
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, p_path);
        return asset;
    }
}
