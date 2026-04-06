using TMPro;
using DiceGame.GameFlow;
using UnityEngine;
using UnityEngine.UI;
using DiceGame.Managers;

namespace DiceGame.UI
{
    public class DiceValueInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_InputField firstDiceInputField;
        [SerializeField] private TMP_InputField secondDiceInputField;
        [SerializeField] private Button rollButton;

        [Header("Constants")]
        private const string VALID_CHARACTERS = "123456";
        private const int MIN_DICE_VALUE = 1;
        private const int MAX_DICE_VALUE = 6;

        void OnEnable()
        {
            EventManager.DiceEvents.OnClearDiceInputs += ClearInputFields;
        }
        void OnDisable()
        {
            EventManager.DiceEvents.OnClearDiceInputs -= ClearInputFields;
        }

        private void Start()
        {
            SetupInputFields();
            SetupRollButton();
        }

        private void SetupInputFields()
        {
            if (firstDiceInputField != null)
            {
                firstDiceInputField.characterLimit = 1;
                firstDiceInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                firstDiceInputField.onValueChanged.AddListener(OnFirstDiceInputChanged);
                firstDiceInputField.onEndEdit.AddListener(ValidateFirstDiceInput);
            }

            if (secondDiceInputField != null)
            {
                secondDiceInputField.characterLimit = 1;
                secondDiceInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                secondDiceInputField.onValueChanged.AddListener(OnSecondDiceInputChanged);
                secondDiceInputField.onEndEdit.AddListener(ValidateSecondDiceInput);
            }
        }

        private void SetupRollButton()
        {
            if (rollButton != null)
            {
                rollButton.onClick.AddListener(OnRollButtonClicked);
            }
        }

        private void OnFirstDiceInputChanged(string value)
        {
            firstDiceInputField.text = FilterInput(value);
        }

        private void OnSecondDiceInputChanged(string value)
        {
            secondDiceInputField.text = FilterInput(value);
        }

        private string FilterInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            foreach (char c in input)
            {
                if (!VALID_CHARACTERS.Contains(c.ToString()))
                {
                    return string.Empty;
                }
            }

            return input;
        }

        private void ValidateFirstDiceInput(string value)
        {
            ValidateAndCorrectInput(firstDiceInputField, value);
        }

        private void ValidateSecondDiceInput(string value)
        {
            ValidateAndCorrectInput(secondDiceInputField, value);
        }

        private void ValidateAndCorrectInput(TMP_InputField inputField, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (int.TryParse(value, out int diceValue))
            {
                if (diceValue < MIN_DICE_VALUE)
                {
                    inputField.text = MIN_DICE_VALUE.ToString();
                }
                else if (diceValue > MAX_DICE_VALUE)
                {
                    inputField.text = MAX_DICE_VALUE.ToString();
                }
            }
            else
            {
                inputField.text = string.Empty;
            }
        }

        private void OnRollButtonClicked()
        {
            if (GameFlowController.Instance != null &&
                GameFlowController.Instance.CurrentState != GameFlowState.Gameplay)
            {
                return;
            }

            if (!TryGetDiceValues(out int[] diceValues)) // dicesValues will assign in TryGetDiceValues method
            {
                Debug.LogWarning("Please enter valid dice values (1-6) in both fields!");
                return;
            }

            EventManager.DiceEvents.OnDiceValuesEntered?.Invoke(diceValues); // dicesValues is assigned in TryGetDiceValues method, if it returns true

            ClearInputFields();
        }

        private bool TryGetDiceValues(out int[] diceValues)
        {
            diceValues = new int[2];

            if (string.IsNullOrEmpty(firstDiceInputField.text) ||
                string.IsNullOrEmpty(secondDiceInputField.text))
            {
                return false;
            }

            if (!int.TryParse(firstDiceInputField.text, out diceValues[0]) ||
                !int.TryParse(secondDiceInputField.text, out diceValues[1]))
            {
                return false;
            }

            if (diceValues[0] < MIN_DICE_VALUE || diceValues[0] > MAX_DICE_VALUE ||
                diceValues[1] < MIN_DICE_VALUE || diceValues[1] > MAX_DICE_VALUE)
            {
                return false;
            }

            return true;
        }

        private void ClearInputFields()
        {
            if (firstDiceInputField != null)
            {
                firstDiceInputField.text = string.Empty;
            }

            if (secondDiceInputField != null)
            {
                secondDiceInputField.text = string.Empty;
            }
        }

        private void OnDestroy()
        {
            if (rollButton != null)
            {
                rollButton.onClick.RemoveListener(OnRollButtonClicked);
            }

            if (firstDiceInputField != null)
            {
                firstDiceInputField.onValueChanged.RemoveListener(OnFirstDiceInputChanged);
                firstDiceInputField.onEndEdit.RemoveListener(ValidateFirstDiceInput);
            }

            if (secondDiceInputField != null)
            {
                secondDiceInputField.onValueChanged.RemoveListener(OnSecondDiceInputChanged);
                secondDiceInputField.onEndEdit.RemoveListener(ValidateSecondDiceInput);
            }
        }
    }
}

