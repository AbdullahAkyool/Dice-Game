using System;

namespace DiceGame.Events
{
    [Serializable]
    public class PlayerEvents
    {
        public Action<int, int> OnPlayerMoveRequested;
        public Action OnPlayerMovementCompleted;
        public Action OnStopPlayerMovementRequested;
        public Action OnResetPlayerPositionRequested;
    }
}
