using System;
using DiceGame.Data;
using UnityEngine;

namespace DiceGame.Events
{
    [Serializable]
    public class InventoryEvents
    {
        public Action<FruitType, int> OnItemAdded;
        public Action<FruitType, int> OnItemRemoved;
    }
}
