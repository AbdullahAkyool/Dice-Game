using DiceGame.Managers;

namespace DiceGame.GameFlow.States
{
    public sealed class CameraToDiceState : IGameState
    {
        private readonly GameSessionContext _ctx;
        private GameFlowStateMachine _machine;

        public CameraToDiceState(GameSessionContext ctx)
        {
            _ctx = ctx;
        }

        public GameStateType StateType => GameStateType.CameraToDice;

        public void OnEnter(GameFlowStateMachine machine)
        {
            _machine = machine;

            if (_ctx.DiceRollController != null && _ctx.PendingDiceValues != null)
            {
                _ctx.DiceRollController.ConfigureDiceTargets(_ctx.PendingDiceValues);
            }

            EventManager.CameraEvents.OnDiceCameraTransitionCompleted += OnDiceCameraReady;
            EventManager.CameraEvents.OnSwitchToDiceCamera?.Invoke();
        }

        public void OnUpdate(GameFlowStateMachine machine)
        {
        }

        public void OnExit(GameFlowStateMachine machine)
        {
            EventManager.CameraEvents.OnDiceCameraTransitionCompleted -= OnDiceCameraReady;
            _machine = null;
        }

        private void OnDiceCameraReady()
        {
            if (_machine != null && _ctx.RollingDiceSettledState != null)
            {
                _machine.ChangeState(_ctx.RollingDiceSettledState);
            }
        }
    }
}
