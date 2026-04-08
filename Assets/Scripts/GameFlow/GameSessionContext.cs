using DiceGame.Dice;
using UnityEngine;

namespace DiceGame.GameFlow
{
    public sealed class GameSessionContext
    {
        public GameFlowController GameFlowController { get; set; }
        public DiceRollController DiceRollController { get; set; }
        public float PlayerCameraTransitionDelay { get; set; }

        public int[] PendingDiceValues { get; set; }

        public IGameState LevelSelectionState { get; set; }
        public IGameState IdleState { get; set; }
        public IGameState CameraToDiceState { get; set; }
        public IGameState RollingDiceSettledState { get; set; }
        public IGameState CameraToPlayerState { get; set; }
        public IGameState MoveState { get; set; }
        public IGameState ResolvingTileState { get; set; }
        public IGameState TurnCleanupState { get; set; }

        public MonoBehaviour CoroutineRunner => GameFlowController;
    }
}
