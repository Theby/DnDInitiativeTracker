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
                SetPositionLabel(value);
            }
        }

        public int Initiative
        {
            get => int.Parse(initiativeInputField.text);
            private set => initiativeInputField.text = value.ToString();
        }

        public CharacterUIData Data { get; private set; }

        public event Action<int> OnRemove;
        public event Action<int, string> OnSelectionChanged;

        public void Initialize()
        {
            characterDropdown.onValueChanged.AddListener(CharacterSelectedHandler);
            removeButton.onClick.AddListener(OnRemoveButtonPressedHandler);
        }

        void OnDestroy()
        {
            Data?.Dispose();
        }

        public void SetData(CharacterUIData data, List<string> characterNames)
        {
            Data = data;

            characterDropdown.ClearOptions();
            characterDropdown.AddOptions(characterNames);

            var dropDownIndex = Data == null ? 0 : characterDropdown.options.FindIndex(x => x.text == Data.Name);
            characterDropdown.value = dropDownIndex;

            Initiative = Data?.Initiative ?? 0;
        }

        void SetPositionLabel(int positionIndex)
        {
            positionLabel.text = positionIndex.ToString();
        }

        void CharacterSelectedHandler(int index)
        {
            var characterName = characterDropdown.options[index].text;
            if (Data != null && Data.Name == characterName)
                return;

            var layoutIndex = PositionIndex - 1;
            OnSelectionChanged?.Invoke(layoutIndex, characterName);
        }

        void OnRemoveButtonPressedHandler()
        {
            OnRemove?.Invoke(PositionIndex);
        }
    }
}
