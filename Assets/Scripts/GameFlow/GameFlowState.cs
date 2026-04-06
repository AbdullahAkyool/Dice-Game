using System;

namespace DiceGame.GameFlow
{
    [Serializable]
    public enum GameFlowState
    {
        LevelSelection = 0,
        LevelTransition = 1,
        Gameplay = 2
    }
}
