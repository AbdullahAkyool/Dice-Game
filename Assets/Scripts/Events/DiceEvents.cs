using System;

namespace DiceGame.Events
{
    [Serializable]
    public class DiceEvents
    {
        public Action<int[]> OnDiceValuesEntered;
        public Action OnDiceRollingStarted;
        public Action OnDiceRollingFinished;
    }
}
