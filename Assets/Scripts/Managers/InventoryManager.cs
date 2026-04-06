using System;
using System.Collections.Generic;
using DiceGame.Data;
using UnityEngine;

namespace DiceGame.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }
        public Dictionary<FruitType, int> Fruits { get; private set; } = new Dictionary<FruitType, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeFruitSlots();
        }

        private void OnEnable()
        {
            EventManager.InventoryEvents.OnInventoryReset += ResetInventory;
            EventManager.InventoryEvents.OnItemAdded += HandleItemAdded;
        }

        private void OnDisable()
        {
            EventManager.InventoryEvents.OnInventoryReset -= ResetInventory;
            EventManager.InventoryEvents.OnItemAdded -= HandleItemAdded;
        }

        private void InitializeFruitSlots()
        {
            Fruits.Clear();
            foreach (FruitType type in Enum.GetValues(typeof(FruitType)))
            {
                if (type == FruitType.None) continue;
                Fruits[type] = 0;

                EventManager.InventoryEvents.OnUpdateItemUIElement?.Invoke(type, 0);
            }
        }

        private void ResetInventory()
        {
            InitializeFruitSlots();
        }
        private void HandleItemAdded(FruitType fruitType, int amount)
        {
            if (amount <= 0) return;
            if (fruitType == FruitType.None) return;

            if (Fruits.ContainsKey(fruitType))
            {
                Fruits[fruitType] += amount;
            }
            else
            {
                Fruits[fruitType] = amount;
            }

            EventManager.InventoryEvents.OnUpdateItemUIElement?.Invoke(fruitType, amount);

            Debug.Log($"Earned {amount} {fruitType}. Total: {Fruits[fruitType]}");
        }
    }
}
