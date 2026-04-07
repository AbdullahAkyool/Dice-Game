using System;
using System.Collections.Generic;

namespace DiceGame.Save
{
    [Serializable]
    public class LevelSaveData
    {
        public int LevelIndex;
        public int PlayerTileIndex;
        public Dictionary<string, int> InventoryByFruitName = new Dictionary<string, int>();
    }
}
