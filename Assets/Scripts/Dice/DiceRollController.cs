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

        private int[] tempDiceValues;
        [SerializeField] private List<DiceController> diceControllers;
        private int diceCompetedCount = 0;

        private void OnEnable()
        {
            EventManager.InventoryEvents.OnCheckEarnableRewards += CheckEarnableRewards;

            EventManager.DiceEvents.OnDiceValuesEntered += HandleDiceValuesEntered;
            EventManager.DiceEvents.OnDiceRollingStarted += StartDiceRolling;
            EventManager.DiceEvents.OnDiceRollingFinished += CheckDicesCompeted;
        }

        private void OnDisable()
        {
            EventManager.InventoryEvents.OnCheckEarnableRewards -= CheckEarnableRewards;

            EventManager.DiceEvents.OnDiceValuesEntered -= HandleDiceValuesEntered;
            EventManager.DiceEvents.OnDiceRollingStarted -= StartDiceRolling;
            EventManager.DiceEvents.OnDiceRollingFinished -= CheckDicesCompeted;
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

        private int CalculateTargetTile(int steps)
        {
            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            int newIndex = currentTileIndex + steps;

            int wrappedIndex = boardGenerator.GetWrappedIndex(newIndex);

            currentTileIndex = wrappedIndex;

            return wrappedIndex;
        }

        private void CheckDicesCompeted()
        {
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

            int total = CalculateTotal(tempDiceValues);

            int targetTileIndex = CalculateTargetTile(total);

            LogRollResult(tempDiceValues, total, targetTileIndex);
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

        public void ResetPosition()
        {
            currentTileIndex = 0;
            Debug.Log("Player position reset to tile 0");
        }
    }
}
