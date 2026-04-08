using System.Collections;
using DiceGame.Managers;
using UnityEngine;

namespace DiceGame.GameFlow.States
{
    public sealed class RollingDiceSettledState : IGameState //UniTask'a gecilecek
    {
        private readonly GameSessionContext _ctx;
        private GameFlowStateMachine _machine;
        private int _diceFinishedCount;
        private Coroutine _delayRoutine;

        public RollingDiceSettledState(GameSessionContext ctx)
        {
            _ctx = ctx;
        }

        public GameStateType StateType => GameStateType.RollingDiceSettled;

        public void OnEnter(GameFlowStateMachine machine)
        {
            _machine = machine;
            _diceFinishedCount = 0;

            int expected = _ctx.DiceRollController != null ? _ctx.DiceRollController.DiceCount : 0;
            if (expected <= 0 || _ctx.PendingDiceValues == null || _ctx.PendingDiceValues.Length == 0)
            {
                machine.ChangeState(_ctx.IdleState);
                return;
            }

            EventManager.DiceEvents.OnDiceRollingFinished += OnSingleDiceFinished;
            EventManager.DiceEvents.OnDiceRollingStarted?.Invoke();

            _ctx.DiceRollController.StartAllDiceRolling();
        }

        public void OnUpdate(GameFlowStateMachine machine)
        {
        }

        public void OnExit(GameFlowStateMachine machine)
        {
            EventManager.DiceEvents.OnDiceRollingFinished -= OnSingleDiceFinished;

            if (_delayRoutine != null && _ctx.CoroutineRunner != null)
            {
                _ctx.CoroutineRunner.StopCoroutine(_delayRoutine);
                _delayRoutine = null;
            }

            _machine = null;
        }

        private void OnSingleDiceFinished()
        {
            if (_machine == null || _ctx.DiceRollController == null)
            {
                return;
            }

            _diceFinishedCount++;
            if (_diceFinishedCount < _ctx.DiceRollController.DiceCount)
            {
                return;
            }

            EventManager.DiceEvents.OnDiceRollingFinished -= OnSingleDiceFinished;
            _diceFinishedCount = 0;

            if (_ctx.CoroutineRunner != null)
            {
                _delayRoutine = _ctx.CoroutineRunner.StartCoroutine(DelayThenGoToPlayerCamera());
            }
            else
            {
                _machine.ChangeState(_ctx.CameraToPlayerState);
            }
        }

        private IEnumerator DelayThenGoToPlayerCamera()
        {
            yield return new WaitForSeconds(_ctx.PlayerCameraTransitionDelay);
            _delayRoutine = null;

            if (_machine != null)
            {
                _machine.ChangeState(_ctx.CameraToPlayerState);
            }
        }
    }
}
