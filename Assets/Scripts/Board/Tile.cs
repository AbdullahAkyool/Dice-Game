using DiceGame.Data;
using DiceGame.Managers;
using DiceGame.Pooling;
using TMPro;
using UnityEngine;

namespace DiceGame.Board
{
    public class Tile : MonoBehaviour, IPoolable
    {
        [Header("References")]
        [SerializeField] private TMP_Text tileNumberText;
        [SerializeField] private TMP_Text rewardText;

        public int TileIndex { get; private set; }
        public TileData TileData { get; private set; }

        public void Initialize(TileData tileData)
        {
            TileData = tileData;
            TileIndex = tileData.index;

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (tileNumberText != null)
            {
                tileNumberText.text = TileIndex.ToString();
            }

            if (TileData.HasReward)
            {
                string fruitName = GetFruitName(TileData.FruitTypeEnum);
                rewardText.text = $"{fruitName} x{TileData.amount}";
            }
            else
            {
                if (rewardText != null)
                {
                    rewardText.text = "";
                }
            }
        }

        private void Reset()
        {
            if (rewardText != null)
            {
                rewardText.text = "";
            }
        }

        private string GetFruitName(FruitType fruitType)
        {
            return fruitType switch
            {
                FruitType.Apple => "Apple",
                FruitType.Pear => "Pear",
                FruitType.Strawberry => "Strawberry",
                _ => "Fruit"
            };
        }

        public Vector3 GetPlayerPosition()
        {
            return transform.position + Vector3.up * 1.5f;
        }

        public void OnSpawn()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawn()
        {
            Reset();
            gameObject.SetActive(false);
        }
    }
}
