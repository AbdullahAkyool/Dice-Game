using System;

namespace DiceGame.GameFlow
{
    public sealed class GameFlowStateMachine
    {
        private IGameState _current;

        public GameSessionContext Context { get; }

        public event Action<GameStateType> StateChanged;

        public GameFlowStateMachine(GameSessionContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IGameState Current => _current;

        public GameStateType CurrentType => _current?.StateType ?? default;

        public void ChangeState(IGameState next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (_current == next)
            {
                return;
            }

            _current?.OnExit(this);
            _current = next;
            _current.OnEnter(this);
            StateChanged?.Invoke(_current.StateType);
        }

        public void Tick()
        {
            _current?.OnUpdate(this);
        }
    }
}
