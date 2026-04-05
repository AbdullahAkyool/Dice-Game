using System.Collections;
using DiceGame.Board;
using DiceGame.Managers;
using UnityEngine;

namespace DiceGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveLerpSpeed = 4f;
        [SerializeField] private float stopDistance = 0.01f;

        private BoardGenerator boardGenerator;
        private Coroutine movementCoroutine;

        private void OnEnable()
        {
            EventManager.DiceEvents.OnPlayerMoveRequested += HandlePlayerMoveRequested;
        }

        private void OnDisable()
        {
            EventManager.DiceEvents.OnPlayerMoveRequested -= HandlePlayerMoveRequested;
        }

        private void Start()
        {
            boardGenerator = BoardGenerator.Instance;
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
                EventManager.DiceEvents.OnPlayerMovementCompleted?.Invoke();
                yield break;
            }

            Vector3 targetPosition = targetTile.GetPlayerPosition();
            targetPosition.x = transform.position.x;
            targetPosition.y = transform.position.y;

            while (Mathf.Abs(targetPosition.z - transform.position.z) > stopDistance)
            {
                float nextZ = Mathf.Lerp(transform.position.z, targetPosition.z, moveLerpSpeed * Time.deltaTime);
                float deltaZ = nextZ - transform.position.z;

                transform.Translate(Vector3.forward * deltaZ, Space.World);

                yield return null;
            }

            Vector3 finalPosition = transform.position;
            finalPosition.z = targetPosition.z;
            transform.position = finalPosition;

            movementCoroutine = null;
            EventManager.DiceEvents.OnPlayerMovementCompleted?.Invoke();
        }
    }
}
