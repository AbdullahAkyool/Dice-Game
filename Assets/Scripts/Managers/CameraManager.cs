using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace DiceGame.Managers
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [SerializeField] private CinemachineBlenderSettings blenderSettings;

        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private CinemachineCamera diceCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventManager.CameraEvents.OnSwitchToDiceCamera += HandleSwitchToDiceCamera;
            EventManager.CameraEvents.OnSwitchToPlayerCamera += HandleSwitchToPlayerCamera;
        }

        private void OnDisable()
        {
            EventManager.CameraEvents.OnSwitchToDiceCamera -= HandleSwitchToDiceCamera;
            EventManager.CameraEvents.OnSwitchToPlayerCamera -= HandleSwitchToPlayerCamera;
        }

        private void HandleSwitchToPlayerCamera()
        {
            StartCoroutine(HandleSwitchToPlayerCameraCO());
        }

        private void HandleSwitchToDiceCamera()
        {
            StartCoroutine(HandleSwitchToDiceCameraCO());
        }
        private IEnumerator HandleSwitchToPlayerCameraCO()
        {
            playerCamera.enabled = true;
            diceCamera.enabled = false;

            int blendIndex = Array.FindIndex(blenderSettings.CustomBlends,
                b => b.From == diceCamera.name && b.To == playerCamera.name);

            yield return new WaitForSeconds(blendIndex >= 0 ? blenderSettings.CustomBlends[blendIndex].Blend.Time + .5f : 1.5f);
        }

        private IEnumerator HandleSwitchToDiceCameraCO()
        {
            playerCamera.enabled = false;
            diceCamera.enabled = true;

            int blendIndex = Array.FindIndex(blenderSettings.CustomBlends,
                b => b.From == playerCamera.name && b.To == diceCamera.name);

            yield return new WaitForSeconds(blendIndex >= 0 ? blenderSettings.CustomBlends[blendIndex].Blend.Time + .5f : 1.5f);

            EventManager.DiceEvents.OnDiceRollingStarted?.Invoke();
        }
    }
}
