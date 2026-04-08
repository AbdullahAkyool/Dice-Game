using System;
using System.Collections.Generic;
using DiceGame.Board;
using DiceGame.Data;
using DiceGame.Dice;
using DiceGame.GameFlow.States;
using DiceGame.Managers;
using DiceGame.Player;
using DiceGame.Save;
using UnityEngine;

namespace DiceGame.GameFlow
{
    public class GameFlowController : MonoBehaviour
    {
        public static GameFlowController Instance { get; private set; }

        [SerializeField] private DiceRollController diceRollController;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private float playerCameraTransitionDelay = 0.5f;

        private GameFlowStateMachine _stateMachine;
        private GameSessionContext _context;

        public GameStateType CurrentStateType => _stateMachine != null ? _stateMachine.CurrentType : default;
        public int CurrentLevelIndex { get; private set; } = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            ResolveGameplayReferences();
            BuildStateMachine();
        }

        private void BuildStateMachine()
        {
            _context = new GameSessionContext
            {
                GameFlowController = this,
                DiceRollController = diceRollController,
                PlayerCameraTransitionDelay = playerCameraTransitionDelay
            };

            var levelSelection = new LevelSelectionState(_context);
            var idle = new IdleState(_context);
            var cameraToDice = new CameraToDiceState(_context);
            var rolling = new RollingDiceSettledState(_context);
            var cameraToPlayer = new CameraToPlayerState(_context);
            var move = new MoveState(_context);
            var resolvingTile = new ResolvingTileState(_context);
            var turnCleanup = new TurnCleanupState(_context);

            _context.LevelSelectionState = levelSelection;
            _context.IdleState = idle;
            _context.CameraToDiceState = cameraToDice;
            _context.RollingDiceSettledState = rolling;
            _context.CameraToPlayerState = cameraToPlayer;
            _context.MoveState = move;
            _context.ResolvingTileState = resolvingTile;
            _context.TurnCleanupState = turnCleanup;

            _stateMachine = new GameFlowStateMachine(_context);
            _stateMachine.StateChanged += OnMachineStateChanged;
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
            {
                _stateMachine.StateChanged -= OnMachineStateChanged;
            }
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
            _stateMachine.ChangeState(_context.LevelSelectionState);
        }

        private void Update()
        {
            _stateMachine?.Tick();
        }

        private void OnMachineStateChanged(GameStateType stateType)
        {
            EventManager.GameFlowEvents.OnStateChanged?.Invoke(stateType);
        }

        private void HandleLevelProgressChanged()
        {
            SaveCurrentLevelIfPossible();
        }

        private void HandleLevelSelectedRequested(int levelIndex)
        {
            TransitionToLevel(levelIndex, false);
        }

        private void HandleOpenLevelSelectionRequested()
        {
            if (_stateMachine != null && _stateMachine.CurrentType == GameStateType.LevelSelection)
            {
                return;
            }

            SaveCurrentLevelIfPossible();
            StopActiveGameplay();
            _stateMachine.ChangeState(_context.LevelSelectionState);
        }

        private void HandleResetCurrentLevelRequested()
        {
            if (CurrentLevelIndex < 0)
            {
                Debug.LogWarning("Reset requested before a level was selected.");
                return;
            }

            TransitionToLevel(CurrentLevelIndex, true);
        }

        private void TransitionToLevel(int levelIndex, bool clearProgress)
        {
            StopActiveGameplay();

            if (clearProgress)
            {
                LevelSaveService.Delete(levelIndex);
            }

            if (BoardGenerator.Instance == null)
            {
                Debug.LogError("BoardGenerator not found; cannot transition level.");
                _stateMachine.ChangeState(_context.LevelSelectionState);
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
            _stateMachine.ChangeState(_context.IdleState);
        }

        private void ApplyLevelSaveData(LevelSaveData data)
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

        private void SaveCurrentLevelIfPossible()
        {
            if (CurrentLevelIndex < 0 || CurrentStateType == GameStateType.LevelSelection)
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

        public void StopActiveGameplay()
        {
            EventManager.CameraEvents.OnSnapToPlayerCameraImmediate?.Invoke();
            EventManager.DiceEvents.OnStopGameplayActivities?.Invoke();
            EventManager.PlayerEvents.OnStopPlayerMovementRequested?.Invoke();
            EventManager.DiceEvents.OnClearDiceInputs?.Invoke();
            _context.PendingDiceValues = null;
        }
    }
}
