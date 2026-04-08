using System;

namespace DiceGame.GameFlow
{
    [Serializable]
    public enum GameStateType
    {
        LevelSelection = 0,
        Idle = 1,
        CameraToDice = 2,
        RollingDiceSettled = 3,
        CameraToPlayer = 4,
        Move = 5,
        ResolvingTile = 6,
        TurnCleanup = 7
    }
}
