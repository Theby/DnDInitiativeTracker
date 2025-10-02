using System;
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

        public int Initiative => int.Parse(initiativeInputField.text);

        public event Action<int> OnRemove;

        public void Initialize(int positionIndex)
        {
            PositionIndex = positionIndex;
            initiativeInputField.text = "0";
            removeButton.onClick.AddListener(OnRemoveButtonPressedHandler);
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
