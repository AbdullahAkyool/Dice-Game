using System.Collections;
using DiceGame.Board;
using DiceGame.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace DiceGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerAnimatorController animatorController;

        [Header("Movement Settings")]
        [FormerlySerializedAs("moveLerpSpeed")]
        [SerializeField] private float moveSpeed = 4f;
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

        private void HandlePlayerMoveRequested(int fromTileIndex, int steps)
        {
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
            }

            movementCoroutine = StartCoroutine(MoveToTileCoroutine(fromTileIndex, steps));
        }

        private IEnumerator MoveToTileCoroutine(int fromTileIndex, int steps)
        {
            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            if (boardGenerator == null)
            {
                movementCoroutine = null;
                EventManager.PlayerEvents.OnPlayerMovementCompleted?.Invoke();
                yield break;
            }

            animatorController.RunAnimation();

            int currentTileIndex = fromTileIndex;

            for (int i = 0; i < steps; i++)
            {
                int nextTileIndex = boardGenerator.GetWrappedPlayableIndex(currentTileIndex, 1);
                Tile nextTile = boardGenerator.GetTile(nextTileIndex);

                if (nextTile == null)
                {
                    Debug.LogWarning($"Target tile {nextTileIndex} not found.");
                    break;
                }

                if (nextTileIndex < currentTileIndex)
                {
                    int wrapRestartTileIndex = boardGenerator.TileCount > BoardGenerator.FirstPlayableTileBoardIndex
                        ? BoardGenerator.FirstPlayableTileBoardIndex
                        : BoardGenerator.StartTileBoardIndex;

                    PlaceOnTileIndex(wrapRestartTileIndex);
                }

                Vector3 targetPosition = nextTile.GetPlayerPosition();
                targetPosition.x = transform.position.x;
                targetPosition.y = transform.position.y;

                while (Mathf.Abs(targetPosition.z - transform.position.z) > stopDistance)
                {
                    Vector3 pos = transform.position;
                    pos.z = Mathf.MoveTowards(pos.z, targetPosition.z, moveSpeed * Time.deltaTime);
                    transform.position = pos;

                    yield return null;
                }

                Vector3 finalPosition = transform.position;
                finalPosition.z = targetPosition.z;
                transform.position = finalPosition;

                currentTileIndex = nextTileIndex;
            }

            animatorController.IdleAnimation();

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
            TeleportToTile(0);
        }

        public void TeleportToTile(int tileIndex) // save dosyasindan gelen tile indexine playeri aninda teleport et, genellikle level resetlenirken veya save dosyasindan gelen degerler uygulandiktan sonra kullanilir
        {
            StopCurrentMovement();

            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            PlaceOnTileIndex(tileIndex);
            animatorController.IdleAnimation();
        }

        private void PlaceOnTileIndex(int tileIndex)
        {
            Tile tile = boardGenerator != null ? boardGenerator.GetTile(tileIndex) : null;
            if (tile == null)
            {
                Debug.LogWarning($"PlaceOnTileIndex: tile {tileIndex} not found.");
                return;
            }

            Vector3 targetPosition = tile.GetPlayerPosition();
            targetPosition.x = transform.position.x;
            targetPosition.y = transform.position.y;
            transform.position = targetPosition;
        }
    }
}
