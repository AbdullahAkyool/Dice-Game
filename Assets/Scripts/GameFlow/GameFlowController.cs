using DiceGame.Board;
using DiceGame.Managers;
using UnityEngine;

namespace DiceGame.GameFlow
{
    public class GameFlowController : MonoBehaviour
    {
        public static GameFlowController Instance { get; private set; }

        public GameFlowState CurrentState { get; private set; }
        public int CurrentLevelIndex { get; private set; } = -1;

        private bool hasInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            EventManager.GameFlowEvents.OnLevelSelectedRequested += HandleLevelSelectedRequested;
            EventManager.GameFlowEvents.OnOpenLevelSelectionRequested += HandleOpenLevelSelectionRequested;
            EventManager.GameFlowEvents.OnResetCurrentLevelRequested += HandleResetCurrentLevelRequested;
        }

        private void OnDisable()
        {
            EventManager.GameFlowEvents.OnLevelSelectedRequested -= HandleLevelSelectedRequested;
            EventManager.GameFlowEvents.OnOpenLevelSelectionRequested -= HandleOpenLevelSelectionRequested;
            EventManager.GameFlowEvents.OnResetCurrentLevelRequested -= HandleResetCurrentLevelRequested;
        }

        private void Start()
        {
            SetState(GameFlowState.LevelSelection);
        }

        private void HandleLevelSelectedRequested(int levelIndex)
        {
            TransitionToLevel(levelIndex, false);
        }

        private void HandleOpenLevelSelectionRequested()
        {
            if (CurrentState == GameFlowState.LevelSelection)
            {
                return;
            }

            StopActiveGameplay();
            SetState(GameFlowState.LevelSelection);
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
            SetState(GameFlowState.LevelTransition);
            StopActiveGameplay();

            if (clearProgress)
            {
                Debug.Log($"Resetting level {levelIndex}. Save deletion can be added here later.");
            }

            if (BoardGenerator.Instance == null)
            {
                Debug.LogError("BoardGenerator not found; cannot transition level.");
                SetState(GameFlowState.LevelSelection);
                return;
            }

            CurrentLevelIndex = levelIndex;
            BoardGenerator.Instance.BuildLevel(levelIndex);

            EventManager.DiceEvents.OnResetGameplayState?.Invoke();
            EventManager.InventoryEvents.OnInventoryReset?.Invoke();
            EventManager.PlayerEvents.OnResetPlayerPositionRequested?.Invoke();
            EventManager.DiceEvents.OnClearDiceInputs?.Invoke();

            SetState(GameFlowState.Gameplay);
        }

        private static void StopActiveGameplay()
        {
            EventManager.CameraEvents.OnSnapToPlayerCameraImmediate?.Invoke();
            EventManager.DiceEvents.OnStopGameplayActivities?.Invoke();
            EventManager.PlayerEvents.OnStopPlayerMovementRequested?.Invoke();
            EventManager.DiceEvents.OnClearDiceInputs?.Invoke();
        }

        private void SetState(GameFlowState nextState)
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
