using System;
using System.Collections.Generic;
using System.IO;
using DiceGame.Data;
using DiceGame.Managers;
using Newtonsoft.Json;
using UnityEngine;

namespace DiceGame.Save
{
    public static class LevelSaveService
    {
        private const string SavesSubfolder = "Saves";

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static bool TryLoad(int levelIndex, out LevelSaveData data) // level indexe gore save dosyasini yukle
        {
            data = null;
            string path = GetFilePath(levelIndex);
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                data = JsonConvert.DeserializeObject<LevelSaveData>(json, JsonSettings);
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Level save load failed for level {levelIndex}: {e.Message}");
                return false;
            }
        }

        public static void Save(int levelIndex, LevelSaveData data) // save datasini kaydet
        {
            if (data == null)
            {
                return;
            }

            data.LevelIndex = levelIndex;
            try
            {
                EnsureSaveDirectoryExists();
                string path = GetFilePath(levelIndex);
                string json = JsonConvert.SerializeObject(data, JsonSettings);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Level save write failed for level {levelIndex}: {e.Message}");
            }
        }

        public static void Delete(int levelIndex) // save dosyasini sil
        {
            string path = GetFilePath(levelIndex);
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Level save delete failed for level {levelIndex}: {e.Message}");
            }
        }

        public static bool HasSave(int levelIndex) // save dosyasi var mi
        {
            return File.Exists(GetFilePath(levelIndex));
        }

        public static LevelSaveData BuildFromCurrentSession(int levelIndex, int playerTileIndex) // kaydedilecek datayi olustur
        {
            var data = new LevelSaveData
            {
                LevelIndex = levelIndex,
                PlayerTileIndex = playerTileIndex,
                InventoryByFruitName = new Dictionary<string, int>()
            };

            if (InventoryManager.Instance != null)
            {
                foreach (var kv in InventoryManager.Instance.Fruits)
                {
                    if (kv.Key == FruitType.None)
                    {
                        continue;
                    }

                    data.InventoryByFruitName[kv.Key.ToString()] = kv.Value;
                }
            }

            return data;
        }

        private static void EnsureSaveDirectoryExists() // save dosyasi yoksa olustur
        {
            string dir = GetSaveDirectoryPath();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private static string GetSaveDirectoryPath() // save dosyasinin yolunu bul
        {
            return Path.Combine(Application.persistentDataPath, SavesSubfolder);
        }

        private static string GetFilePath(int levelIndex) // level indexe gore save dosyasini bul
        {
            return Path.Combine(GetSaveDirectoryPath(), $"level_{levelIndex}.json");
        }
    }
}
