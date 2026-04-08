using System.Collections;
using DiceGame.Data;
using DiceGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class CurrencyEarnEffectController : MonoBehaviour
    {
        [SerializeField] private GameObject particlePiecesParent;

        private const float PieceDelayStep = 0.1f;
        private const float ScaleUpDuration = 0.3f;
        private const float PreMoveDelay = 0.2f;
        private const float MoveDuration = 0.8f;
        private const float ScaleDownDuration = 0.3f;

        private RectTransform[] particlePieces;
        private Image[] particlePieceImages;
        private Vector2[] initialPositions;
        private Quaternion[] initialLocalRotations;
        private Vector3[] initialScales;

        private RectTransform currentTargetPoint;
        private FruitType currentFruitType = FruitType.None;
        private int pendingEarnAmount;

        private void OnEnable()
        {
            EventManager.InventoryEvents.OnRewardTileReached += HandleRewardTileReached;
        }

        private void OnDisable()
        {
            EventManager.InventoryEvents.OnRewardTileReached -= HandleRewardTileReached;
        }

        private void Awake()
        {
            if (particlePiecesParent == null)
            {
                Debug.LogWarning("ParticlePiecesParent reference is missing.", this);
                enabled = false;
                return;
            }

            CacheParticlePieces();
            ResetEffect();
        }

        public void Set(FruitType fruitType, RectTransform targetPoint)
        {
            currentFruitType = fruitType;
            currentTargetPoint = targetPoint;
            UpdateParticlePieceSprites(fruitType);
        }

        public void Play()
        {
            if (particlePieces == null || particlePieces.Length == 0)
            {
                return;
            }

            if (currentFruitType == FruitType.None || currentTargetPoint == null)
            {
                Debug.LogWarning("Earn effect is missing fruit type or target point.", this);
                return;
            }

            StopAllCoroutines();
            ResetParticleTransforms();
            particlePiecesParent.SetActive(true);
            StartCoroutine(PlayEffectRoutine());
        }

        private void HandleRewardTileReached(FruitType fruitType, int amount)
        {
            if (amount <= 0 || fruitType == FruitType.None)
            {
                return;
            }

            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager instance not found.");
                return;
            }

            RectTransform targetPoint = InventoryManager.Instance.GetInventoryItemUIElementTargetPoint(fruitType);
            if (targetPoint == null)
            {
                Debug.LogWarning($"Target point could not be found for {fruitType}, reward will be added immediately.", this);
                EventManager.InventoryEvents.OnItemAdded?.Invoke(fruitType, amount);
                return;
            }

            pendingEarnAmount = amount;
            Set(fruitType, targetPoint);
            Play();
        }

        private void CacheParticlePieces()
        {
            int childCount = particlePiecesParent.transform.childCount;
            particlePieces = new RectTransform[childCount];
            particlePieceImages = new Image[childCount];
            initialPositions = new Vector2[childCount];
            initialLocalRotations = new Quaternion[childCount];
            initialScales = new Vector3[childCount];

            for (int i = 0; i < childCount; i++)
            {
                RectTransform rt = particlePiecesParent.transform.GetChild(i).GetComponent<RectTransform>();
                particlePieces[i] = rt;
                particlePieceImages[i] = rt.GetComponent<Image>();
                initialPositions[i] = rt.anchoredPosition;
                initialLocalRotations[i] = rt.localRotation;
                initialScales[i] = rt.localScale;
            }
        }

        private void UpdateParticlePieceSprites(FruitType fruitType)
        {
            if (DatabaseManager.Instance == null || DatabaseManager.Instance.FruitDatabase == null)
            {
                Debug.LogWarning("FruitDatabase is missing, particle sprites could not be updated.", this);
                return;
            }

            Sprite fruitIcon = DatabaseManager.Instance.FruitDatabase.GetFruitIcon(fruitType);
            if (fruitIcon == null)
            {
                Debug.LogWarning($"Fruit icon could not be found for {fruitType}.", this);
                return;
            }

            for (int i = 0; i < particlePieceImages.Length; i++)
            {
                if (particlePieceImages[i] != null)
                {
                    particlePieceImages[i].sprite = fruitIcon;
                }
            }
        }

        private IEnumerator PlayEffectRoutine()
        {
            for (int i = 0; i < particlePieces.Length; i++)
            {
                StartCoroutine(AnimateParticleRoutine(particlePieces[i], i, i * PieceDelayStep));
            }

            yield return new WaitForSeconds(GetTotalEffectDuration());

            EventManager.InventoryEvents.OnItemAdded?.Invoke(currentFruitType, pendingEarnAmount);
            ResetEffect();
        }

        private float GetTotalEffectDuration()
        {
            if (particlePieces == null || particlePieces.Length == 0)
            {
                return 0f;
            }

            float lastPieceDelay = (particlePieces.Length - 1) * PieceDelayStep;
            return lastPieceDelay + ScaleUpDuration + PreMoveDelay + MoveDuration + ScaleDownDuration;
        }

        private void ResetEffect()
        {
            ResetParticleTransforms();

            if (particlePiecesParent != null)
            {
                particlePiecesParent.SetActive(false);
            }

            currentFruitType = FruitType.None;
            currentTargetPoint = null;
            pendingEarnAmount = 0;
        }

        private void ResetParticleTransforms()
        {
            if (particlePieces == null)
            {
                return;
            }

            for (int i = 0; i < particlePieces.Length; i++)
            {
                RectTransform particlePiece = particlePieces[i];
                particlePiece.anchoredPosition = initialPositions[i];
                particlePiece.localRotation = initialLocalRotations[i];
                particlePiece.localScale = Vector3.zero;
            }
        }

        private IEnumerator AnimateParticleRoutine(RectTransform particlePiece, int index, float delay)
        {
            yield return new WaitForSeconds(delay);

            yield return StartCoroutine(LerpScale(particlePiece, Vector3.zero, initialScales[index], ScaleUpDuration));

            yield return new WaitForSeconds(PreMoveDelay);

            float elapsed = 0f;
            Vector2 startPos = particlePiece.anchoredPosition;
            Quaternion startRot = particlePiece.localRotation;
            Vector2 targetPosition = GetParticleTargetPosition(particlePiece);

            while (elapsed < MoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / MoveDuration);
                float easeT = t * t * t;

                particlePiece.anchoredPosition = Vector2.Lerp(startPos, targetPosition, easeT);
                particlePiece.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, t);
                yield return null;
            }

            particlePiece.anchoredPosition = targetPosition;
            particlePiece.localRotation = Quaternion.identity;
            yield return StartCoroutine(LerpScale(particlePiece, initialScales[index], Vector3.zero, ScaleDownDuration));
        }

        private Vector2 GetParticleTargetPosition(RectTransform particlePiece)
        {
            if (currentTargetPoint == null)
            {
                return particlePiece.anchoredPosition;
            }

            RectTransform parentRect = particlePiece.parent as RectTransform;
            Canvas canvas = currentTargetPoint.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, currentTargetPoint.position);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, uiCamera, out Vector2 localPoint);
            return localPoint;
        }

        private IEnumerator LerpScale(RectTransform target, Vector3 start, Vector3 end, float time)
        {
            float elapsed = 0f;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / time));
                yield return null;
            }

            target.localScale = end;
        }
    }
}
