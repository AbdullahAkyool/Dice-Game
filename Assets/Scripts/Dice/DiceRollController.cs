using UnityEngine;
using DiceGame.Managers;
using DiceGame.Board;
using DiceGame.Data;
using System.Collections.Generic;

namespace DiceGame.Dice
{
    public class DiceRollController : MonoBehaviour
    {
        [Header("References")]
        private BoardGenerator boardGenerator;

        private int currentTileIndex = 0;
        private int pendingTargetTileIndex = 0;

        private int[] tempDiceValues;
        [SerializeField] private List<DiceController> diceControllers;
        private int diceCompetedCount = 0;

        private void OnEnable()
        {
            EventManager.InventoryEvents.OnCheckEarnableRewards += CheckEarnableRewards;

            EventManager.DiceEvents.OnDiceValuesEntered += HandleDiceValuesEntered;
            EventManager.DiceEvents.OnDiceRollingStarted += StartDiceRolling;
            EventManager.DiceEvents.OnDiceRollingFinished += CheckDicesCompeted;
            EventManager.DiceEvents.OnStopGameplayActivities += StopActiveGameplay;
            EventManager.PlayerEvents.OnPlayerMovementCompleted += HandlePlayerMovementCompleted;
            EventManager.CameraEvents.OnPlayerCameraTransitionCompleted += HandlePlayerCameraTransitionCompleted;

            EventManager.DiceEvents.OnResetGameplayState += ResetGameplayState;

        }

        private void OnDisable()
        {
            EventManager.InventoryEvents.OnCheckEarnableRewards -= CheckEarnableRewards;

            EventManager.DiceEvents.OnDiceValuesEntered -= HandleDiceValuesEntered;
            EventManager.DiceEvents.OnDiceRollingStarted -= StartDiceRolling;
            EventManager.DiceEvents.OnDiceRollingFinished -= CheckDicesCompeted;
            EventManager.DiceEvents.OnStopGameplayActivities -= StopActiveGameplay;
            EventManager.PlayerEvents.OnPlayerMovementCompleted -= HandlePlayerMovementCompleted;
            EventManager.CameraEvents.OnPlayerCameraTransitionCompleted -= HandlePlayerCameraTransitionCompleted;

            EventManager.DiceEvents.OnResetGameplayState -= ResetGameplayState;
        }

        private void Start()
        {
            boardGenerator = BoardGenerator.Instance;
        }

        private void HandleDiceValuesEntered(int[] diceValues)
        {
            tempDiceValues = diceValues;
            if (diceControllers == null) return;

            for (int i = 0; i < diceControllers.Count; i++)
            {
                if (i < diceValues.Length)
                {
                    diceControllers[i].TargetFaceValue = diceValues[i];
                }
            }

            EventManager.CameraEvents.OnSwitchToDiceCamera?.Invoke();

        }

        private void StartDiceRolling()
        {
            if (diceControllers == null) return;
            if (tempDiceValues == null) return;

            foreach (DiceController diceController in diceControllers)
            {
                diceController.RollToCurrentTarget();
            }
        }

        private int CalculateTotal(int[] diceValues)
        {
            int total = 0;
            foreach (int value in diceValues)
            {
                total += value;
            }
            return total;
        }

        private int CalculateTargetTileIndex(int steps)
        {
            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            int newIndex = currentTileIndex + steps;
            return boardGenerator.GetWrappedIndex(newIndex);
        }

        private void CheckDicesCompeted()
        {
            if (tempDiceValues == null || tempDiceValues.Length == 0)
            {
                return;
            }

            if (diceControllers == null || diceControllers.Count == 0)
            {
                return;
            }

            diceCompetedCount++;

            if (diceCompetedCount >= diceControllers.Count)
            {
                diceCompetedCount = 0;
                EventManager.CameraEvents.OnSwitchToPlayerCamera?.Invoke();
                return;
            }
        }

        private void CheckEarnableRewards()
        {
            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            Tile targetTile = boardGenerator.GetTile(currentTileIndex);
            if (targetTile == null) return;

            if (targetTile.TileData.HasReward)
            {
                int fruitCount = targetTile.TileData.amount;
                FruitType fruitType = targetTile.TileData.FruitTypeEnum;

                EventManager.InventoryEvents.OnItemAdded?.Invoke(fruitType, fruitCount);
            }
        }

        private void HandlePlayerCameraTransitionCompleted()
        {
            if (tempDiceValues == null || tempDiceValues.Length == 0)
            {
                return;
            }

            int total = CalculateTotal(tempDiceValues);
            pendingTargetTileIndex = CalculateTargetTileIndex(total);

            EventManager.PlayerEvents.OnPlayerMoveRequested?.Invoke(pendingTargetTileIndex);
        }

        private void HandlePlayerMovementCompleted()
        {
            if (tempDiceValues == null || tempDiceValues.Length == 0)
            {
                return;
            }

            int total = CalculateTotal(tempDiceValues);
            currentTileIndex = pendingTargetTileIndex;

            LogRollResult(tempDiceValues, total, currentTileIndex);
            CheckEarnableRewards();
            tempDiceValues = null;
        }

        private void LogRollResult(int[] diceValues, int total, int targetTileIndex)
        {
            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            string diceValuesStr = string.Join(" + ", diceValues);

            Tile targetTile = boardGenerator.GetTile(targetTileIndex);
            string tileInfo = targetTile != null ? $"Tile {targetTileIndex}" : "Unknown Tile";

            if (targetTile != null && targetTile.TileData.HasReward)
            {
                tileInfo += $" (Reward: {targetTile.TileData.FruitTypeEnum} x{targetTile.TileData.amount})";
            }

            Debug.Log($"<color=cyan>Dice Roll: [{diceValuesStr}] = {total} → Landed on {tileInfo}</color>");
        }

        public void ResetGameplayState()
        {
            currentTileIndex = 0;
            pendingTargetTileIndex = 0;
            diceCompetedCount = 0;
            tempDiceValues = null;
        }

        private void StopActiveGameplay()
        {
            pendingTargetTileIndex = currentTileIndex;
            diceCompetedCount = 0;
            tempDiceValues = null;
        }
    }
}
