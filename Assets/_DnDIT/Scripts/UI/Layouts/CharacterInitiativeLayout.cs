using System;
using System.Collections.Generic;
using DnDInitiativeTracker.GameData;
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

        public event Action<int> OnRemove;

        public void Initialize(int positionIndex)
        {
            PositionIndex = positionIndex;

            characterDropdown.ClearOptions();
            removeButton.onClick.AddListener(OnRemoveButtonPressedHandler);
        }

        public void SetData(CharacterData data, List<string> characterNames)
        {
            characterDropdown.AddOptions(characterNames);

            var dropDownIndex = data == null ? 0 : characterDropdown.options.FindIndex(x => x.text == data.Name);
            characterDropdown.value = dropDownIndex;

            Initiative = data?.Initiative ?? 0;
        }

        void SetPositionLabel(int positionIndex)
        {
            positionLabel.text = positionIndex.ToString();
        }

        void OnRemoveButtonPressedHandler()
        {
            OnRemove?.Invoke(PositionIndex);
        }
    }
}
