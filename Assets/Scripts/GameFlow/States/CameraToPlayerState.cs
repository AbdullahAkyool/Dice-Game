using DiceGame.Managers;

namespace DiceGame.GameFlow.States
{
    public sealed class CameraToPlayerState : IGameState
    {
        private readonly GameSessionContext _ctx;
        private GameFlowStateMachine _machine;

        public CameraToPlayerState(GameSessionContext ctx)
        {
            _ctx = ctx;
        }

        public GameStateType StateType => GameStateType.CameraToPlayer;

        public void OnEnter(GameFlowStateMachine machine)
        {
            _machine = machine;
            EventManager.CameraEvents.OnPlayerCameraTransitionCompleted += OnPlayerCameraReady;
            EventManager.CameraEvents.OnSwitchToPlayerCamera?.Invoke();
        }

        public void OnUpdate(GameFlowStateMachine machine)
        {
        }

        public void OnExit(GameFlowStateMachine machine)
        {
            EventManager.CameraEvents.OnPlayerCameraTransitionCompleted -= OnPlayerCameraReady;
            _machine = null;
        }

        private void OnPlayerCameraReady()
        {
            if (_machine != null && _ctx.MoveState != null)
            {
                _machine.ChangeState(_ctx.MoveState);
            }
        }
    }
}
