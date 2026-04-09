using DiceGame.Managers;

namespace DiceGame.GameFlow.States
{
    public sealed class MoveState : IGameState
    {
        private readonly GameSessionContext _ctx;
        private GameFlowStateMachine _machine;

        public MoveState(GameSessionContext ctx)
        {
            _ctx = ctx;
        }

        public GameStateType StateType => GameStateType.Move;

        public void OnEnter(GameFlowStateMachine machine)
        {
            _machine = machine;

            if (_ctx.DiceRollController == null || _ctx.PendingDiceValues == null || _ctx.PendingDiceValues.Length == 0)
            {
                machine.ChangeState(_ctx.IdleState);
                return;
            }

            int total = _ctx.DiceRollController.CalculateTotal(_ctx.PendingDiceValues);
            int target = _ctx.DiceRollController.ComputeTargetTileIndex(total);
            _ctx.DiceRollController.SetPendingTargetTileIndex(target);

            int fromTile = _ctx.DiceRollController.CurrentTileIndex;

            if(UIManager.Instance != null)
            {
                UIManager.Instance.SetDiceElementsActive(false);
            }

            EventManager.PlayerEvents.OnPlayerMovementCompleted += OnMovementCompleted;
            EventManager.PlayerEvents.OnPlayerMoveRequested?.Invoke(fromTile, total);
        }

        public void OnUpdate(GameFlowStateMachine machine)
        {
        }

        public void OnExit(GameFlowStateMachine machine)
        {
            EventManager.PlayerEvents.OnPlayerMovementCompleted -= OnMovementCompleted;
            _machine = null;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetDiceElementsActive(true);
            }
        }

        private void OnMovementCompleted()
        {
            if (_machine != null && _ctx.ResolvingTileState != null)
            {
                _machine.ChangeState(_ctx.ResolvingTileState);
            }
        }
    }
}
