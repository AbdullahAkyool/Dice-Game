using DiceGame.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace DiceGame.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels (CanvasGroup)")]
        [SerializeField] private CanvasGroup levelSelectPanel;
        [SerializeField] private CanvasGroup gameplayPanel;

        [Header("Gameplay reset")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button menuButton;

        [Header("Dice Elements Holder")]
        [SerializeField] private GameObject[] diceElementsHolder;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(RequestResetCurrentLevel);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OpenLevelMenu);
            }
        }

        private void OnDestroy()
        {
            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(RequestResetCurrentLevel);
            }

            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(OpenLevelMenu);
            }
        }

        private void OnEnable()
        {
            EventManager.GameFlowEvents.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            EventManager.GameFlowEvents.OnStateChanged -= HandleStateChanged;
        }

        private void Start()
        {
            SetPanel(levelSelectPanel, true);
            SetPanel(gameplayPanel, false);
        }

        private void HandleStateChanged(GameStateType state)
        {
            if (state == GameStateType.LevelSelection)
            {
                SetPanel(levelSelectPanel, true);
                SetPanel(gameplayPanel, false);
            }
            else
            {
                SetPanel(levelSelectPanel, false);
                SetPanel(gameplayPanel, true);
            }
        }

        private void RequestResetCurrentLevel()
        {
            EventManager.GameFlowEvents.OnResetCurrentLevelRequested?.Invoke();
        }

        public void OpenLevelMenu()
        {
            EventManager.GameFlowEvents.OnOpenLevelSelectionRequested?.Invoke();
        }

        public void SetDiceElementsActive(bool active)
        {
            if (diceElementsHolder == null)
            {
                return;
            }

            foreach (GameObject holder in diceElementsHolder)
            {
                if (holder != null)
                {
                    holder.SetActive(active);
                }
            }
        }

        private static void SetPanel(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
