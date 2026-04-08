using System.Collections.Generic;
using DiceGame.Board;
using DiceGame.Data;
using DiceGame.Managers;
using UnityEngine;

namespace DiceGame.Dice
{
    public class DiceRollController : MonoBehaviour
    {
        [Header("References")]
        private BoardGenerator boardGenerator;

        private int currentTileIndex;
        private int pendingTargetTileIndex;

        [SerializeField] private List<DiceController> diceControllers;

        public int CurrentTileIndex => currentTileIndex;

        public int DiceCount => diceControllers != null ? diceControllers.Count : 0;

        public void ApplyTileIndexFromSave(int tileIndex)
        {
            currentTileIndex = tileIndex;
            pendingTargetTileIndex = tileIndex;
        }

        private void OnEnable()
        {
            EventManager.InventoryEvents.OnCheckEarnableRewards += CheckEarnableRewards;
            EventManager.DiceEvents.OnStopGameplayActivities += AbortActiveTurnSession;
            EventManager.DiceEvents.OnResetGameplayState += ResetGameplayState;
        }

        private void OnDisable()
        {
            EventManager.InventoryEvents.OnCheckEarnableRewards -= CheckEarnableRewards;
            EventManager.DiceEvents.OnStopGameplayActivities -= AbortActiveTurnSession;
            EventManager.DiceEvents.OnResetGameplayState -= ResetGameplayState;
        }

        private void Start()
        {
            boardGenerator = BoardGenerator.Instance;
        }

        public void ConfigureDiceTargets(int[] diceValues)
        {
            if (diceControllers == null || diceValues == null)
            {
                return;
            }

            for (int i = 0; i < diceControllers.Count; i++)
            {
                if (i < diceValues.Length)
                {
                    diceControllers[i].TargetFaceValue = diceValues[i];
                }
            }
        }

        public void StartAllDiceRolling()
        {
            if (diceControllers == null)
            {
                return;
            }

            foreach (DiceController diceController in diceControllers)
            {
                diceController.RollToCurrentTarget();
            }
        }

        public int CalculateTotal(int[] diceValues)
        {
            if (diceValues == null || diceValues.Length == 0)
            {
                return 0;
            }

            int total = 0;
            foreach (int value in diceValues)
            {
                total += value;
            }

            return total;
        }

        public int ComputeTargetTileIndex(int steps)
        {
            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            return boardGenerator.GetWrappedPlayableIndex(currentTileIndex, steps);
        }

        public void SetPendingTargetTileIndex(int index)
        {
            pendingTargetTileIndex = index;
        }

        public void CommitPlayerTileAfterMove()
        {
            currentTileIndex = pendingTargetTileIndex;
        }

        public void LogRollResultForCurrentTurn(int[] diceValues)
        {
            if (diceValues == null || diceValues.Length == 0)
            {
                return;
            }

            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            int total = CalculateTotal(diceValues);
            string diceValuesStr = string.Join(" + ", diceValues);

            Tile targetTile = boardGenerator.GetTile(currentTileIndex);
            string tileInfo = DescribeLandingTile(targetTile, currentTileIndex);

            if (targetTile != null
                && !BoardGenerator.IsStartTileIndex(currentTileIndex)
                && targetTile.TileData.HasReward)
            {
                tileInfo += $" (Reward: {targetTile.TileData.FruitTypeEnum} x{targetTile.TileData.amount})";
            }

            Debug.Log($"<color=cyan>Dice Roll: [{diceValuesStr}] = {total} → Landed on {tileInfo}</color>");
        }

        public void ApplyRewardsAtCurrentTile()
        {
            CheckEarnableRewards();
        }

        private static string DescribeLandingTile(Tile tile, int boardIndex)
        {
            if (tile == null)
            {
                return "Unknown Tile";
            }

            if (BoardGenerator.IsStartTileIndex(boardIndex))
            {
                return "START";
            }

            return $"Tile {boardIndex}";
        }

        private void CheckEarnableRewards()
        {
            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            if (BoardGenerator.IsStartTileIndex(currentTileIndex))
            {
                return;
            }

            Tile targetTile = boardGenerator.GetTile(currentTileIndex);
            if (targetTile == null)
            {
                return;
            }

            if (targetTile.TileData.HasReward)
            {
                int fruitCount = targetTile.TileData.amount;
                FruitType fruitType = targetTile.TileData.FruitTypeEnum;

                if (EventManager.InventoryEvents.OnRewardTileReached != null)
                {
                    EventManager.InventoryEvents.OnRewardTileReached.Invoke(fruitType, fruitCount);
                }
                else
                {
                    EventManager.InventoryEvents.OnItemAdded?.Invoke(fruitType, fruitCount);
                }
            }
        }

        public void ResetGameplayState()
        {
            currentTileIndex = 0;
            pendingTargetTileIndex = 0;
        }

        public void AbortActiveTurnSession()
        {
            pendingTargetTileIndex = currentTileIndex;
        }
    }
}
