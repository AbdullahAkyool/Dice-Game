using System;
using System.Collections.Generic;
using DiceGame.Data;
using DiceGame.UI;
using UnityEngine;

namespace DiceGame.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }
        public Dictionary<FruitType, int> Fruits { get; private set; } = new Dictionary<FruitType, int>();
        [SerializeField] private List<InventoryItemUIElement> inventoryUIElements;

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

        public void ApplyFromSave(Dictionary<FruitType, int> values) // save dosyasindan gelen degerleri uygula
        {
            if (values == null)
            {
                return;
            }

            InitializeFruitSlots();
            foreach (var kv in values)
            {
                if (kv.Key == FruitType.None)
                {
                    continue;
                }

                Fruits[kv.Key] = kv.Value;
            }

            EventManager.InventoryEvents.OnInventoryForceRefresh?.Invoke();
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

        public InventoryItemUIElement GetInventoryItemUIElement(FruitType fruitType)
        {
            if (inventoryUIElements == null || inventoryUIElements.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < inventoryUIElements.Count; i++)
            {
                InventoryItemUIElement itemUIElement = inventoryUIElements[i];
                if (itemUIElement != null && itemUIElement.FruitType == fruitType)
                {
                    return itemUIElement;
                }
            }

            return null;
        }

        public RectTransform GetInventoryItemUIElementTargetPoint(FruitType fruitType)
        {
            InventoryItemUIElement itemUIElement = GetInventoryItemUIElement(fruitType);
            return itemUIElement != null ? itemUIElement.TargetPoint : null;
        }
    }
}
