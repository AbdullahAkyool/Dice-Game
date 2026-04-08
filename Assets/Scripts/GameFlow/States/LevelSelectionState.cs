namespace DiceGame.GameFlow.States
{
    public sealed class LevelSelectionState : IGameState
    {
        private readonly GameSessionContext _ctx;

        public LevelSelectionState(GameSessionContext ctx)
        {
            _ctx = ctx;
        }

        public GameStateType StateType => GameStateType.LevelSelection;

        public void OnEnter(GameFlowStateMachine machine)
        {
            _ctx.GameFlowController?.StopActiveGameplay();
        }

        public void OnUpdate(GameFlowStateMachine machine)
        {
        }

        public void OnExit(GameFlowStateMachine machine)
        {
        }
    }
}
