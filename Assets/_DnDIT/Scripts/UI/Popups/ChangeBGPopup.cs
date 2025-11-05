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
        [SerializeField] Button addNewButton;
        [SerializeField] TextMeshProUGUI fileNameLabel;
        [SerializeField] TMP_Dropdown nameDropdown;
        [SerializeField] Button removeButton;
        [SerializeField] RawImage previewImage;
        [SerializeField] Button applyButton;
        [Header("Events")]
        [SerializeField] UnityEvent onClose;
        [SerializeField] UnityEvent onAddNew;
        [SerializeField] UnityEvent<string> onRemove;
        [SerializeField] UnityEvent<string> onSelectionChanged;
        [SerializeField] UnityEvent<TextureUIData> onApply;

        DMScreenData _data;
        TextureUIData _loadedBgUIData;

        public override void Initialize()
        {
            closeButton.onClick.AddListener(onClose.Invoke);
            addNewButton.onClick.AddListener(onAddNew.Invoke);
            nameDropdown.onValueChanged.AddListener(SelectionChangedHandler);
            removeButton.onClick.AddListener(RemoveHandler);
            applyButton.onClick.AddListener(ApplyHandler);
        }

        public void SetData(DMScreenData data)
        {
            _data = data;

            nameDropdown.ClearOptions();
            nameDropdown.AddOptions(_data.BackgroundNames);

            var dropDownIndex = nameDropdown.options.FindIndex(x => x.text == _data.CurrentConfigurationUIData.CurrentBackground.Name);
            nameDropdown.SetValueWithoutNotify(dropDownIndex);
        }

        public override void Show()
        {
            base.Show();

            ShowDropDown(_data.CurrentConfigurationUIData.CurrentBackground);
        }

        public void Refresh()
        {
            fileNameLabel.text = _loadedBgUIData.Name;
            previewImage.texture = _loadedBgUIData.Data;
        }

        public void ShowDropDown(TextureUIData backgroundUIData)
        {
            fileNameLabel.gameObject.SetActive(false);
            removeButton.gameObject.SetActive(false);
            nameDropdown.gameObject.SetActive(true);

            _loadedBgUIData = backgroundUIData;
            Refresh();
        }

        public void ShowNewBackground(TextureUIData backgroundUIData)
        {
            fileNameLabel.gameObject.SetActive(true);
            removeButton.gameObject.SetActive(true);
            nameDropdown.gameObject.SetActive(false);

            _loadedBgUIData = backgroundUIData;
            Refresh();
        }

        void SelectionChangedHandler(int index)
        {
            var bgName = _data.BackgroundNames[index];
            onSelectionChanged.Invoke(bgName);
        }

        void RemoveHandler()
        {
            var index = nameDropdown.value;
            var bgName = _data.BackgroundNames[index];
            onRemove.Invoke(bgName);
        }

        void ApplyHandler()
        {
            onApply.Invoke(_loadedBgUIData);
        }
    }
}
