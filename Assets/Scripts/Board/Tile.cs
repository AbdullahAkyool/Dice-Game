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
        public bool IsStartTile { get; private set; }

        public void Initialize(TileData tileData, int boardIndex)
        {
            TileData = tileData;
            TileIndex = boardIndex;
            IsStartTile = tileData != null && tileData.TileTypeEnum == TileType.Start;

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (IsStartTile)
            {
                if (tileNumberText != null)
                {
                    tileNumberText.text = "START";
                }

                ClearRewardVisuals();
                return;
            }

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
                ClearRewardVisuals();
            }
        }

        private void ClearRewardVisuals()
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

        private void ClearForPool()
        {
            ClearRewardVisuals();
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
            ClearForPool();
            gameObject.SetActive(false);
        }
    }
}
