using DiceGame.Data;
using DiceGame.Managers;
using DiceGame.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.Board
{
    public class Tile : MonoBehaviour, IPoolable
    {
        [Header("References")]
        [SerializeField] private TMP_Text tileNumberText;
        [SerializeField] private TMP_Text rewardCountText;
        [SerializeField] private SpriteRenderer rewardIcon;

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
                if (rewardIcon != null)
                {
                    rewardIcon.enabled = true;
                }

                Sprite fruitIcon = GetFruitIcon(TileData.FruitTypeEnum);
                rewardIcon.sprite = fruitIcon;
                rewardCountText.text = $"x{TileData.amount}";
            }
            else
            {
                if (rewardCountText != null)
                {
                    rewardCountText.text = "";
                }

                if (rewardIcon != null)
                {
                    rewardIcon.enabled = false;
                }
            }
        }

        private void Reset()
        {
            if (rewardCountText != null)
            {
                rewardCountText.text = "";
            }
        }

        private Sprite GetFruitIcon(FruitType fruitType)
        {
            if (DatabaseManager.Instance == null || DatabaseManager.Instance.FruitDatabase == null)
            {
                Debug.LogError("DatabaseManager or FruitDatabase not assigned.");
                return null;
            }

            return DatabaseManager.Instance.FruitDatabase.GetFruitIcon(fruitType);
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
