using System;
using System.Collections.Generic;
using DiceGame.Board;
using DiceGame.Data;
using DiceGame.Dice;
using DiceGame.Managers;
using DiceGame.Player;
using DiceGame.Save;
using UnityEngine;

namespace DiceGame.GameFlow
{
    public class GameFlowController : MonoBehaviour
    {
        public static GameFlowController Instance { get; private set; }

        public GameFlowState CurrentState { get; private set; }
        public int CurrentLevelIndex { get; private set; } = -1;

        [SerializeField] private DiceRollController diceRollController;
        [SerializeField] private PlayerController playerController;

        private bool hasInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            ResolveGameplayReferences();
        }

        private void ResolveGameplayReferences()
        {
            if (diceRollController == null)
            {
                Debug.LogError("DiceRollController not found; cannot start gameplay.");
            }

            if (playerController == null)
            {
                Debug.LogError("PlayerController not found; cannot start gameplay.");
            }
        }

        private void OnApplicationQuit()
        {
            SaveCurrentLevelIfPossible();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveCurrentLevelIfPossible();
            }
        }

        private void OnEnable()
        {
            EventManager.GameFlowEvents.OnLevelSelectedRequested += HandleLevelSelectedRequested;
            EventManager.GameFlowEvents.OnOpenLevelSelectionRequested += HandleOpenLevelSelectionRequested;
            EventManager.GameFlowEvents.OnResetCurrentLevelRequested += HandleResetCurrentLevelRequested;
            EventManager.GameFlowEvents.OnLevelProgressChanged += HandleLevelProgressChanged;
        }

        private void OnDisable()
        {
            EventManager.GameFlowEvents.OnLevelSelectedRequested -= HandleLevelSelectedRequested;
            EventManager.GameFlowEvents.OnOpenLevelSelectionRequested -= HandleOpenLevelSelectionRequested;
            EventManager.GameFlowEvents.OnResetCurrentLevelRequested -= HandleResetCurrentLevelRequested;
            EventManager.GameFlowEvents.OnLevelProgressChanged -= HandleLevelProgressChanged;
        }

        private void Start()
        {
            ResolveGameplayReferences();
            SetState(GameFlowState.LevelSelection);
        }

        private void HandleLevelProgressChanged() // level progress kaydetmek istendiginde
        {
            SaveCurrentLevelIfPossible();
        }

        private void HandleLevelSelectedRequested(int levelIndex) // level secilmesi istendiginde
        {
            TransitionToLevel(levelIndex, false);
        }

        private void HandleOpenLevelSelectionRequested() // level selection menu acilmasi istendiginde
        {
            if (CurrentState == GameFlowState.LevelSelection)
            {
                return;
            }

            SaveCurrentLevelIfPossible();
            StopActiveGameplay();
            SetState(GameFlowState.LevelSelection);
        }

        private void HandleResetCurrentLevelRequested() // mevcut level sifirlanmasi istendiginde
        {
            if (CurrentLevelIndex < 0)
            {
                Debug.LogWarning("Reset requested before a level was selected.");
                return;
            }

            TransitionToLevel(CurrentLevelIndex, true);
        }

        private void TransitionToLevel(int levelIndex, bool clearProgress) // level gecisi yapmak istendiginde, clearProgress true ise mevcut save silinir
        {
            SetState(GameFlowState.LevelTransition);
            StopActiveGameplay();

            if (clearProgress)
            {
                LevelSaveService.Delete(levelIndex);
            }

            if (BoardGenerator.Instance == null)
            {
                Debug.LogError("BoardGenerator not found; cannot transition level.");
                SetState(GameFlowState.LevelSelection);
                return;
            }

            CurrentLevelIndex = levelIndex;
            BoardGenerator.Instance.BuildLevel(levelIndex);

            bool loadedFromSave = false;
            if (!clearProgress && LevelSaveService.TryLoad(levelIndex, out LevelSaveData saveData) && saveData != null)
            {
                ApplyLevelSaveData(saveData);
                loadedFromSave = true;
            }

            if (!loadedFromSave)
            {
                EventManager.DiceEvents.OnResetGameplayState?.Invoke();
                EventManager.InventoryEvents.OnInventoryReset?.Invoke();
                EventManager.PlayerEvents.OnResetPlayerPositionRequested?.Invoke();
            }

            EventManager.DiceEvents.OnClearDiceInputs?.Invoke();

            SetState(GameFlowState.Gameplay);
        }

        private void ApplyLevelSaveData(LevelSaveData data) // save datasini gameplaye uygula
        {
            var inv = new Dictionary<FruitType, int>();
            if (data.InventoryByFruitName != null)
            {
                foreach (var kv in data.InventoryByFruitName)
                {
                    if (Enum.TryParse(kv.Key, out FruitType fruitType) && fruitType != FruitType.None)
                    {
                        inv[fruitType] = kv.Value;
                    }
                }
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ApplyFromSave(inv);
            }

            if (diceRollController != null)
            {
                diceRollController.ApplyTileIndexFromSave(data.PlayerTileIndex);
            }

            if (playerController != null)
            {
                playerController.TeleportToTile(data.PlayerTileIndex);
            }
        }

        private void SaveCurrentLevelIfPossible() // mevcut level kaydedilebilir durumdaysa kaydet
        {
            if (CurrentLevelIndex < 0 || CurrentState != GameFlowState.Gameplay)
            {
                return;
            }

            if (diceRollController == null)
            {
                return;
            }

            LevelSaveData data = LevelSaveService.BuildFromCurrentSession(
                CurrentLevelIndex,
                diceRollController.CurrentTileIndex);

            LevelSaveService.Save(CurrentLevelIndex, data);
        }

        private static void StopActiveGameplay() // aktif gameplay aktivitelerini durdur
        {
            EventManager.CameraEvents.OnSnapToPlayerCameraImmediate?.Invoke();
            EventManager.DiceEvents.OnStopGameplayActivities?.Invoke();
            EventManager.PlayerEvents.OnStopPlayerMovementRequested?.Invoke();
            EventManager.DiceEvents.OnClearDiceInputs?.Invoke();
        }

        private void SetState(GameFlowState nextState) // gameplay durumunu degistir
        {
            if (hasInitialized && CurrentState == nextState)
            {
                return;
            }

            CurrentState = nextState;
            hasInitialized = true;
            EventManager.GameFlowEvents.OnStateChanged?.Invoke(CurrentState);
        }
    }
}
