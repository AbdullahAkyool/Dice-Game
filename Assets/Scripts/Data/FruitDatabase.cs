using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceGame.Data
{
    [CreateAssetMenu(fileName = "NewFruitDatabase", menuName = "Database/Fruit Database")]
    public class FruitDatabase : ScriptableObject
    {
        public List<FruitData> fruits;

        public FruitData GetFruit(FruitType fruitType)
        {
            return fruits.Find(fruit => fruit.fruitType == fruitType);
        }

        public Sprite GetFruitIcon(FruitType fruitType)
        {
            var fruitData = GetFruit(fruitType);
            return fruitData != null ? fruitData.fruitIcon : null;
        }
    }

    [Serializable]
    public class FruitData
    {
        public FruitType fruitType;
        public Sprite fruitIcon;
    }

    public enum FruitType
    {
        None = 0,
        Apple = 1,
        Pear = 2,
        Strawberry = 3
    }
}

