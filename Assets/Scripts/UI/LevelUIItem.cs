using DiceGame.Data;
using DiceGame.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.UI
{
    public class LevelUIItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleLabel;

        private LevelData levelData;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnClicked);
            }
            else
            {
                Debug.LogError("LevelUIItem: button not found.");
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }
        }

        public void Initialize(LevelData data, string title)
        {
            levelData = data;

            if (titleLabel != null)
            {
                titleLabel.text = title;
            }
        }

        private void OnClicked()
        {
            if (levelData == null)
            {
                Debug.LogWarning("LevelUIItem has no LevelData.");
                return;
            }

            EventManager.GameFlowEvents.OnLevelSelectedRequested?.Invoke(levelData.levelIndex);
        }
    }
}
