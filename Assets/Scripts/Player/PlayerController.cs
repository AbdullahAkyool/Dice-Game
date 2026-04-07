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
                Vector3 pos = transform.position;
                pos.z = Mathf.MoveTowards(pos.z, targetPosition.z, moveSpeed * Time.deltaTime);
                transform.position = pos;

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
            TeleportToTile(0);
        }

        public void TeleportToTile(int tileIndex) // save dosyasindan gelen tile indexine playeri aninda teleport et, genellikle level resetlenirken veya save dosyasindan gelen degerler uygulandiktan sonra kullanilir
        {
            StopCurrentMovement();

            if (boardGenerator == null)
            {
                boardGenerator = BoardGenerator.Instance;
            }

            Tile tile = boardGenerator != null ? boardGenerator.GetTile(tileIndex) : null;
            if (tile == null)
            {
                Debug.LogWarning($"TeleportToTile: tile {tileIndex} not found.");
                return;
            }

            Vector3 targetPosition = tile.GetPlayerPosition();
            targetPosition.x = transform.position.x;
            targetPosition.y = transform.position.y;
            transform.position = targetPosition;

            animatorController.IdleAnimation();
        }
    }
}
