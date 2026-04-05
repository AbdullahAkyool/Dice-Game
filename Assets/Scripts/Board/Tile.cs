using DiceGame.Data;
using DiceGame.Managers;
using TMPro;
using UnityEngine;

namespace DiceGame.Board
{
    public class Tile : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text tileNumberText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private MeshRenderer meshRenderer;

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

            if (meshRenderer != null)
            {
                meshRenderer.material.color = TileData.TileColor;
            }

            if (TileData.HasReward)
            {
                var fruitData = DatabaseManager.Instance.FruitDatabase.GetFruit(TileData.FruitTypeEnum);
                if (fruitData != null && meshRenderer != null)
                {
                    meshRenderer.material.color = fruitData.tileColor;

                    string fruitName = GetFruitName(TileData.FruitTypeEnum);
                    rewardText.text = $"{fruitName} x{TileData.amount}";
                }
            }
            else
            {
                if (rewardText != null)
                {
                    rewardText.text = "";
                }
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
    }
}
