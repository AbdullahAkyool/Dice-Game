namespace DiceGame.GameFlow.States
{
    public sealed class ResolvingTileState : IGameState
    {
        private readonly GameSessionContext _ctx;

        public ResolvingTileState(GameSessionContext ctx)
        {
            _ctx = ctx;
        }

        public GameStateType StateType => GameStateType.ResolvingTile;

        public void OnEnter(GameFlowStateMachine machine)
        {
            if (_ctx.DiceRollController == null || _ctx.PendingDiceValues == null || _ctx.PendingDiceValues.Length == 0)
            {
                machine.ChangeState(_ctx.TurnCleanupState);
                return;
            }

            _ctx.DiceRollController.CommitPlayerTileAfterMove();
            _ctx.DiceRollController.LogRollResultForCurrentTurn(_ctx.PendingDiceValues);
            _ctx.DiceRollController.ApplyRewardsAtCurrentTile();

            machine.ChangeState(_ctx.TurnCleanupState);
        }

        public void OnUpdate(GameFlowStateMachine machine)
        {
        }

        public void OnExit(GameFlowStateMachine machine)
        {
        }
    }
}
