using DiceGame.Data;
using DiceGame.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class InventoryItemUIElement : MonoBehaviour
    {
        [SerializeField] private FruitType fruitType;
        public FruitType FruitType => fruitType;

        private int itemCount;

        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemCountText;

        private void Start()
        {
            UpdateItemCount();
            SetItemUIElement(fruitType);
        }

        void OnEnable()
        {
            EventManager.InventoryEvents.OnItemAdded += IncreaseItemCount;
        }

        void OnDisable()
        {
            EventManager.InventoryEvents.OnItemAdded -= IncreaseItemCount;
        }

        private void SetItemUIElement(FruitType fruitType)
        {
            itemIcon.sprite = DatabaseManager.Instance.FruitDatabase.GetFruit(fruitType).fruitIcon;
            UpdateItemCount();
        }

        private void IncreaseItemCount(FruitType fruitType, int amount)
        {
            if (fruitType != FruitType) return;
            itemCount += amount;
        }

        private void UpdateItemCount()
        {
            if (InventoryManager.Instance == null)
            {
                return;
            }

            if (!InventoryManager.Instance.Fruits.TryGetValue(fruitType, out int count))
            {
                count = 0;
            }

            itemCount = count;

            if (itemCountText == null) return;
            itemCountText.text = itemCount.ToString();
        }
    }
}
