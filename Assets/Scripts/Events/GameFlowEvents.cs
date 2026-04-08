using System;
using DiceGame.GameFlow;

namespace DiceGame.Events
{
    [Serializable]
    public class GameFlowEvents
    {
        public Action<int> OnLevelSelectedRequested; // level selection from menu
        public Action OnOpenLevelSelectionRequested; // open level selection menu
        public Action OnResetCurrentLevelRequested; // reset current level
        public Action<GameStateType> OnStateChanged;
        public Action OnLevelProgressChanged; // level progress changed, save level progress when this is invoked
    }
}
