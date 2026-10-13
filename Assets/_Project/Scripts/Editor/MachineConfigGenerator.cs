using MiniFactory.Gameplay;
using UnityEditor;
using UnityEngine;

namespace MiniFactory.Editor
{
    public static class MachineConfigGenerator
    {
        public static void GenerateMachineConfigs()
        {
            string folder = "Assets/_Project/Configs/Machines";
            
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Configs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Configs");
            
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/_Project/Configs", "Machines");

            CreateConfig(folder, "Machine_Drill", "Drill", "tile_0060", 0, 1f, 10, 1.5f, 1.2f);
            CreateConfig(folder, "Machine_Furnace", "Furnace", "tile_0080", 50, 5f, 50, 1.5f, 1.25f);
            CreateConfig(folder, "Machine_Press", "Press", "tile_0100", 200, 20f, 200, 1.6f, 1.3f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("[MiniFactory] Generated 3 machine configs in Assets/_Project/Configs/Machines/");
        }

        private static void CreateConfig(string folder, string id, string name, string tileName,
            int unlock, float income, int upgradeCost, float costMult, float incomeMult)
        {
            string path = $"{folder}/{id}.asset";
            
            var existing = AssetDatabase.LoadAssetAtPath<MachineConfig>(path);
            if (existing != null)
            {
                Debug.Log($"[MiniFactory] Config {id} already exists, skipping.");
                return;
            }

            var config = ScriptableObject.CreateInstance<MachineConfig>();
            config.MachineId = id;
            config.DisplayName = name;
            config.Icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/Tiles/TinyFactory/Tiles/{tileName}.png");
            config.UnlockCost = unlock;
            config.BaseIncomePerSecond = income;
            config.BaseUpgradeCost = upgradeCost;
            config.UpgradeCostMultiplier = costMult;
            config.IncomePerLevelMultiplier = incomeMult;

            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[MiniFactory] Created {id} config.");
        }
    }
}
