using System;
using System.Collections.Generic;
using DnDInitiativeTracker.UIData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class CharacterInitiativeLayout : MonoBehaviour
    {
        [SerializeField] Button removeButton;
        [SerializeField] TextMeshProUGUI positionLabel;
        [SerializeField] TMP_Dropdown characterDropdown;
        [SerializeField] TMP_InputField initiativeInputField;

        int _positionIndex;
        public int PositionIndex
        {
            get => _positionIndex;
            set
            {
                _positionIndex = value;
                positionLabel.text = $"{_positionIndex}";
            }
        }

        public int Initiative
        {
            get => int.Parse(initiativeInputField.text);
            private set => initiativeInputField.text = value.ToString();
        }

        public CharacterUIData LoadedCharacter { get; private set; }

        public event Action<int> OnRemove;
        public event Action<int, string> OnSelectionChanged;

        public void Initialize()
        {
            removeButton.onClick.AddListener(OnRemoveButtonPressedHandler);
            characterDropdown.onValueChanged.AddListener(CharacterSelectedHandler);
        }

        public void SetData(int positionIndex, CharacterUIData characterUIData, int initiative, List<string> characterNames)
        {
            PositionIndex = positionIndex;
            LoadedCharacter = characterUIData;

            characterDropdown.ClearOptions();
            characterDropdown.AddOptions(characterNames);

            var dropDownIndex = LoadedCharacter == null ? 0 : characterDropdown.options.FindIndex(x => x.text == LoadedCharacter.Name);
            characterDropdown.SetValueWithoutNotify(dropDownIndex);

            Initiative = initiative;
        }

        public void UpdateCharacter(CharacterUIData characterUIData)
        {
            LoadedCharacter = characterUIData;
        }

        void OnRemoveButtonPressedHandler()
        {
            var index = PositionIndex - 1;
            OnRemove?.Invoke(index);
        }

        void CharacterSelectedHandler(int dropdownIndex)
        {
            var index = PositionIndex - 1;
            var characterName = characterDropdown.options[dropdownIndex].text;
            OnSelectionChanged?.Invoke(index, characterName);
        }
    }
}
