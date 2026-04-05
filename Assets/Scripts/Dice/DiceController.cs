using System.Collections;
using DiceGame.Managers;
using UnityEngine;

namespace DiceGame.Dice
{
    public class DiceController : MonoBehaviour
    {
        private const float DefaultSpawnHeightAboveLanding = 5f;

        [Header("Target")]
        private int targetFaceValue;
        public int TargetFaceValue { get => targetFaceValue; set => targetFaceValue = value; }

        [Header("Spawn")]
        [SerializeField] private Vector3 landingWorldPosition;
        [SerializeField, Range(5f, 60f)] private float cornerTiltAngle = 28f;

        [Header("Drop")]
        [SerializeField] private float dropDuration = 0.32f;
        [SerializeField] private float spinMultiplier = 2f;

        [Header("Bounce")]
        [SerializeField] private float bounceDuration = 0.3f;
        [SerializeField] private float bounceHeight = 0.6f;

        [Header("Face Mapping")]
        [SerializeField] private Vector3[] faceUpEulerAngles;

        private Rigidbody rb;
        private Coroutine rollRoutine;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        [ContextMenu("Roll To Current Target")]
        public void RollToCurrentTarget()
        {
            if (!Application.isPlaying) return;

            StartTargetedRoll(targetFaceValue);
        }

        public void StartTargetedRoll(int faceValue) // Belirlenen yuzeye gore roll baslat
        {
            targetFaceValue = Mathf.Clamp(faceValue, 1, 6);

            if (rollRoutine != null)
                StopCoroutine(rollRoutine);

            rollRoutine = StartCoroutine(RollRoutine());
        }

        private IEnumerator RollRoutine()
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 spawn = GetSpawnWorldPosition(); // Spawn pozisyonu set
            Quaternion dropStartRot = RandomCornerTiltRotation(); // Capraz dusus icin random rotasyon
            rb.MovePosition(spawn);
            rb.MoveRotation(dropStartRot);

            yield return new WaitForFixedUpdate(); // bir fixedupdate bekle ki fizik engine spawn pozunu islesin

            Quaternion dropEndRot = MultiplyRandomSpin(dropStartRot); // Duserken random spin atsin diye

            yield return AnimateDrop(spawn, landingWorldPosition, dropStartRot, dropEndRot); // Spawn noktasindan dusus noktasina animasyon

            float yaw = rb.rotation.eulerAngles.y; // y ekseninde degisiklik istemiyoruz
            Quaternion finalRot = Quaternion.Euler(GetTargetEulerForFace(targetFaceValue, yaw)); // istenen yuzeyin rotasyonu

            yield return AnimateBounce(landingWorldPosition, finalRot); // ziplama ve hedef rotasyona lerp

            SnapPose(landingWorldPosition, finalRot); // Son pozisyona set et (animasyon tam oturmazsa diye)

            yield return new WaitForFixedUpdate();

            rb.isKinematic = true;
            rollRoutine = null;

            EventManager.DiceEvents.OnDiceRollingFinished?.Invoke();
        }

        private Vector3 GetSpawnWorldPosition()
        {
            return landingWorldPosition + Vector3.up * DefaultSpawnHeightAboveLanding;
        }

        private Quaternion MultiplyRandomSpin(Quaternion baseRot)
        {
            return baseRot * Quaternion.Euler(
                Random.Range(160f, 220f) * spinMultiplier,
                Random.Range(60f, 120f),
                Random.Range(160f, 220f) * spinMultiplier);
        }

        private void SnapPose(Vector3 position, Quaternion rotation)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.MovePosition(position);
            rb.MoveRotation(rotation);
            transform.SetPositionAndRotation(position, rotation);
        }

        private IEnumerator AnimateDrop(Vector3 from, Vector3 to, Quaternion fromRot, Quaternion toRot)
        {
            float elapsed = 0f;
            while (elapsed < dropDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dropDuration);
                float posT = t * t;

                rb.MovePosition(Vector3.Lerp(from, to, posT));
                rb.MoveRotation(Quaternion.Slerp(fromRot, toRot, t));
                yield return null;
            }

            rb.MovePosition(to);
            rb.MoveRotation(toRot);
        }

        private IEnumerator AnimateBounce(Vector3 groundPos, Quaternion targetRot)
        {
            Quaternion fromRot = rb.rotation;
            float elapsed = 0f;

            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / bounceDuration);
                float lift = Mathf.Sin(t * Mathf.PI) * bounceHeight;
                float rotT = EaseOutQuad(t);

                rb.MovePosition(groundPos + Vector3.up * lift);
                rb.MoveRotation(Quaternion.Slerp(fromRot, targetRot, rotT));
                yield return null;
            }

            rb.MovePosition(groundPos);
            rb.MoveRotation(targetRot);
        }

        private static float EaseOutQuad(float t)
        {
            float u = 1f - t;
            return 1f - u * u;
        }

        private Quaternion RandomCornerTiltRotation()
        {
            float tiltX = Random.value > 0.5f ? cornerTiltAngle : -cornerTiltAngle;
            float tiltZ = Random.value > 0.5f ? cornerTiltAngle : -cornerTiltAngle;
            float yaw = Random.Range(0f, 360f);
            return Quaternion.Euler(tiltX, yaw, tiltZ);
        }

        private Vector3 GetTargetEulerForFace(int faceValue, float preserveYaw)
        {
            int i = Mathf.Clamp(faceValue - 1, 0, faceUpEulerAngles.Length - 1);
            Vector3 euler = faceUpEulerAngles[i];
            euler.y = preserveYaw;
            return euler;
        }
    }
}
