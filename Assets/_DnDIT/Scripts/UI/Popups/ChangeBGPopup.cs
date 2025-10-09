using DnDInitiativeTracker.UIData;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class ChangeBGPopup : Panel
    {
        [Header("UI")]
        [SerializeField] Button closeButton;
        [SerializeField] TMP_Dropdown nameDropdown;
        [SerializeField] Button addNewButton;
        [Header("Events")]
        [SerializeField] UnityEvent onClose;
        [SerializeField] UnityEvent<int> onSelectionChanged;
        [SerializeField] UnityEvent onAddNew;

        DMScreenData _data;

        public override void Initialize()
        {
            closeButton.onClick.AddListener(onClose.Invoke);
            nameDropdown.onValueChanged.AddListener(onSelectionChanged.Invoke);
            addNewButton.onClick.AddListener(onAddNew.Invoke);
        }

        public void SetData(DMScreenData data)
        {
            _data = data;
        }

        public override void Show()
        {
            base.Show();

            Refresh();
        }

        public void Refresh()
        {
            nameDropdown.ClearOptions();
            nameDropdown.AddOptions(_data.BackgroundNames);

            var dropDownIndex = nameDropdown.options.FindIndex(x => x.text == _data.CurrentBackground.Name);
            nameDropdown.value = dropDownIndex;
        }
    }
}
