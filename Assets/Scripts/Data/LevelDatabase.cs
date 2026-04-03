using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceGame.Data
{
    [CreateAssetMenu(fileName = "NewLevelDatabase", menuName = "Database/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        public List<LevelData> levels;

        public LevelData GetLevel(int levelIndex)
        {
            return levels.Find(level => level.levelIndex == levelIndex);
        }
    }

    [Serializable]
    public class LevelData
    {
        public int levelIndex;
        public TextAsset mapJsonFile;
    }
}
