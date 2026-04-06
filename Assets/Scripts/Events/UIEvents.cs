using System;
using UnityEngine;

namespace DiceGame.Events
{
    [Serializable]
    public class UIEvents
    {
        public Action<int> OnLevelSelected;
    }
}
