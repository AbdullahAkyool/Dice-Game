using System;
using DiceGame.Data;
using UnityEngine;

namespace DiceGame.Events
{
    [Serializable]
    public class InventoryEvents
    {
        public Action OnCheckEarnableRewards;
        public Action<FruitType, int> OnItemAdded;
        public Action<FruitType, int> OnUpdateItemUIElement;
        public Action OnInventoryReset;
    }
}
