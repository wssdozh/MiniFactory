using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MiniFactory.Services
{
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int coins;
        public long lastSaveTimestamp;
        public List<SavedMachine> machines = new();
        public SavedBoost boost;
    }

    [Serializable]
    public class SavedMachine
    {
        public string id;
        public bool unlocked;
        public int level;
    }

    [Serializable]
    public class SavedBoost
    {
        public bool active;
        public float remainingSeconds;
        public float multiplier;
    }

    public class SaveService
    {
        private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
        private string TempPath => SavePath + ".tmp";

        public SaveData Load()
        {
            if (File.Exists(SavePath) == false)
                return new SaveData();

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveService] Loaded save v{data.version}: {data.coins} coins, {data.machines.Count} machines.");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to load save: {e.Message}");
                return new SaveData();
            }
        }

        public void Save(SaveData data)
        {
            try
            {
                data.lastSaveTimestamp = DateTime.UtcNow.Ticks;
                string json = JsonUtility.ToJson(data, true);
                
                File.WriteAllText(TempPath, json);
                
                if (File.Exists(SavePath))
                    File.Replace(TempPath, SavePath, null);
                else
                    File.Move(TempPath, SavePath);

                Debug.Log($"[SaveService] Saved v{data.version}: {data.coins} coins.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to save: {e.Message}");
            }
        }
    }
}
