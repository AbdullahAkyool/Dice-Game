namespace DiceGame.GameFlow
{
    public interface IGameState
    {
        GameStateType StateType { get; }

        void OnEnter(GameFlowStateMachine machine);
        void OnUpdate(GameFlowStateMachine machine);
        void OnExit(GameFlowStateMachine machine);
    }
}
