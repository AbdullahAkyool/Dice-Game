using System;
using UnityEngine;

namespace DiceGame.Events
{
    [Serializable]
    public class CameraEvents
    {
        public Action OnSwitchToPlayerCamera;
        public Action OnSwitchToDiceCamera;
        public Action OnPlayerCameraTransitionCompleted;
    }
}
