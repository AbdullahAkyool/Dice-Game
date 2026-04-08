using DiceGame.Managers;

namespace DiceGame.GameFlow.States
{
    public sealed class IdleState : IGameState
    {
        private readonly GameSessionContext _ctx;
        private GameFlowStateMachine _machine;

        public IdleState(GameSessionContext ctx)
        {
            _ctx = ctx;
        }

        public GameStateType StateType => GameStateType.Idle;

        public void OnEnter(GameFlowStateMachine machine)
        {
            EventManager.DiceEvents.OnDiceValuesEntered += HandleDiceValuesEntered;
            _machine = machine;
        }

        public void OnUpdate(GameFlowStateMachine machine)
        {
        }

        public void OnExit(GameFlowStateMachine machine)
        {
            EventManager.DiceEvents.OnDiceValuesEntered -= HandleDiceValuesEntered;
            _machine = null;
        }

        private void HandleDiceValuesEntered(int[] diceValues)
        {
            if (diceValues == null || diceValues.Length == 0)
            {
                return;
            }

            _ctx.PendingDiceValues = diceValues;
            _machine.ChangeState(_ctx.CameraToDiceState);
        }
    }
}
