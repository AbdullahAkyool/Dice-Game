using System;
using System.Collections.Generic;
using DiceGame.Managers;
using UnityEngine;

namespace DiceGame.Data
{
    [Serializable]
    public class MapData
    {
        public List<TileData> tiles;
    }

    [Serializable]
    public class TileData
    {
        public int index;
        public string type;
        public string fruitType;
        public int amount;
        public Color TileColor { get { return GetTileColor(GetFruitType(fruitType)); } }
        public TileType TileTypeEnum { get { return GetTileType(type); } }
        public FruitType FruitTypeEnum { get { return GetFruitType(fruitType); } }

        public bool IsEmpty => TileTypeEnum == TileType.Empty;
        public bool HasReward => TileTypeEnum != TileType.Empty && amount > 0;

        private Color GetTileColor(FruitType fruitType)
        {
            if(fruitType == FruitType.None) return Color.white;
            return DatabaseManager.Instance.FruitDatabase.GetFruit(fruitType).tileColor;
        }

        private TileType GetTileType(string type)
        {
            return type?.ToLower() switch
            {
                "empty" => TileType.Empty,
                "fruit" => TileType.Fruit,
                _ => TileType.None
            };
        }

        private FruitType GetFruitType(string fruitType)
        {
            return fruitType?.ToLower() switch
            {
                "apple" => FruitType.Apple,
                "pear" => FruitType.Pear,
                "strawberry" => FruitType.Strawberry,
                _ => FruitType.None
            };
        }
    }

    public enum TileType
    {
        None = 0,
        Empty = 1,
        Fruit = 2
    }
}
