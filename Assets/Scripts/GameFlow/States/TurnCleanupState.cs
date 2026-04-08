using DiceGame.Managers;

namespace DiceGame.GameFlow.States
{
    public sealed class TurnCleanupState : IGameState
    {
        private readonly GameSessionContext _ctx;

        public TurnCleanupState(GameSessionContext ctx)
        {
            _ctx = ctx;
        }

        public GameStateType StateType => GameStateType.TurnCleanup;

        public void OnEnter(GameFlowStateMachine machine)
        {
            _ctx.PendingDiceValues = null;
            EventManager.GameFlowEvents.OnLevelProgressChanged?.Invoke();
            machine.ChangeState(_ctx.IdleState);
        }

        public void OnUpdate(GameFlowStateMachine machine)
        {
        }

        public void OnExit(GameFlowStateMachine machine)
        {
        }
    }
}
