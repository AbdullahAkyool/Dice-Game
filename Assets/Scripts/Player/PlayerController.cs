using System.Collections;
using DiceGame.Board;
using DiceGame.Managers;
using UnityEngine;

namespace DiceGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerAnimatorController animatorController;

        [Header("Movement Settings")]
        [SerializeField] private float moveLerpSpeed = 4f;
        [SerializeField] private float stopDistance = 0.01f;

        private BoardGenerator boardGenerator;
        private Coroutine movementCoroutine;

        void Awake()
        {
            animatorController = GetComponent<PlayerAnimatorController>();
        }

        private void OnEnable()
        {
            EventManager.PlayerEvents.OnPlayerMoveRequested += HandlePlayerMoveRequested;
            EventManager.PlayerEvents.OnStopPlayerMovementRequested += StopCurrentMovement;
            EventManager.PlayerEvents.OnResetPlayerPositionRequested += ResetToStartTile;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.OnPlayerMoveRequested -= HandlePlayerMoveRequested;
            EventManager.PlayerEvents.OnStopPlayerMovementRequested -= StopCurrentMovement;
            EventManager.PlayerEvents.OnResetPlayerPositionRequested -= ResetToStartTile;
        }

        private void Start()
        {
            boardGenerator = BoardGenerator.Instance;
            animatorController.IdleAnimation();
        }

        private void HandlePlayerMoveRequested(int targetTileIndex)
        {
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
            }

            movementCoroutine = StartCoroutine(MoveToTileCoroutine(targetTileIndex));
        }

        private IEnumerator MoveToTileCoroutine(int targetTileIndex)
        {
            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            Tile targetTile = boardGenerator != null ? boardGenerator.GetTile(targetTileIndex) : null;
            if (targetTile == null)
            {
                Debug.LogWarning($"Target tile {targetTileIndex} not found.");
                movementCoroutine = null;
                EventManager.PlayerEvents.OnPlayerMovementCompleted?.Invoke();
                yield break;
            }

            Vector3 targetPosition = targetTile.GetPlayerPosition();
            targetPosition.x = transform.position.x;
            targetPosition.y = transform.position.y;

            animatorController.RunAnimation();

            while (Mathf.Abs(targetPosition.z - transform.position.z) > stopDistance)
            {
                float nextZ = Mathf.Lerp(transform.position.z, targetPosition.z, moveLerpSpeed * Time.deltaTime);
                float deltaZ = nextZ - transform.position.z;

                transform.Translate(Vector3.forward * deltaZ, Space.World);

                yield return null;
            }

            animatorController.IdleAnimation();

            Vector3 finalPosition = transform.position;
            finalPosition.z = targetPosition.z;
            transform.position = finalPosition;

            movementCoroutine = null;

            EventManager.PlayerEvents.OnPlayerMovementCompleted?.Invoke();
        }

        private void StopCurrentMovement()
        {
            if (movementCoroutine == null)
            {
                return;
            }

            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
            animatorController.IdleAnimation();
        }

        private void ResetToStartTile()
        {
            StopCurrentMovement();

            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            Tile startTile = boardGenerator != null ? boardGenerator.GetTile(0) : null;
            if (startTile == null)
            {
                Debug.LogWarning("ResetToStartTile: tile 0 not found.");
                return;
            }

            Vector3 targetPosition = startTile.GetPlayerPosition();
            targetPosition.x = transform.position.x;
            targetPosition.y = transform.position.y;
            transform.position = targetPosition;

            animatorController.IdleAnimation();
        }
    }
}
