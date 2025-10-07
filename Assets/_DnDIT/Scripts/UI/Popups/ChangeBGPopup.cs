using System.Collections.Generic;
using DnDInitiativeTracker.UIData;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class ChangeBGPopup : Panel
    {
        [SerializeField] Button closeButton;
        [SerializeField] TMP_Dropdown nameDropdown;
        [SerializeField] Button addNewButton;
        [Header("Events")]
        [SerializeField] UnityEvent onClose;
        [SerializeField] UnityEvent<int> onSelectionChanged;
        [SerializeField] UnityEvent onAddNew;

        public override void Initialize()
        {
            closeButton.onClick.AddListener(onClose.Invoke);
            nameDropdown.onValueChanged.AddListener(onSelectionChanged.Invoke);
            addNewButton.onClick.AddListener(onAddNew.Invoke);
        }

        public void SetData(string currentName, List<string> backgroundNames)
        {
            nameDropdown.ClearOptions();
            nameDropdown.AddOptions(backgroundNames);

            var dropDownIndex = nameDropdown.options.FindIndex(x => x.text == currentName);
            nameDropdown.value = dropDownIndex;
        }
    }
}
