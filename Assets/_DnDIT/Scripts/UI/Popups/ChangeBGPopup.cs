using DnDInitiativeTracker.UIData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class ChangeBGPopup : Panel
    {
        [Header("UI")]
        [SerializeField] Button closeButton;
        [SerializeField] SelectAssetLayout selectAssetLayout;
        [SerializeField] RawImage previewImage;
        [SerializeField] Button applyButton;
        [Header("Events")]
        [SerializeField] UnityEvent onClose;
        [SerializeField] UnityEvent onAddNew;
        [SerializeField] UnityEvent onRemove;
        [SerializeField] UnityEvent<string> onSelectionChanged;
        [SerializeField] UnityEvent<TextureUIData> onApply;

        DMScreenData _data;
        TextureUIData _loadedBgUIData;

        public override void Initialize()
        {
            closeButton.onClick.AddListener(onClose.Invoke);
            applyButton.onClick.AddListener(ApplyHandler);

            selectAssetLayout.Initialize();
            selectAssetLayout.OnAddNew += onAddNew.Invoke;
            selectAssetLayout.OnSelectionChanged += onSelectionChanged.Invoke;
            selectAssetLayout.OnRemove += onRemove.Invoke;
        }

        public void SetData(DMScreenData data)
        {
            _data = data;

            selectAssetLayout.SetData(_data.CurrentConfigurationUIData.CurrentBackground.Name, _data.BackgroundNames);
        }

        public override void Show()
        {
            base.Show();

            ShowDropDown(_data.CurrentConfigurationUIData.CurrentBackground);
        }

        public void Refresh()
        {
            previewImage.texture = _loadedBgUIData.Data;
        }

        public void ShowDropDown(TextureUIData backgroundUIData)
        {
            selectAssetLayout.ShowDropDown();

            _loadedBgUIData = backgroundUIData;
            Refresh();
        }

        public void ShowNewBackground(TextureUIData backgroundUIData)
        {
            selectAssetLayout.ShowNewAsset(backgroundUIData.Name);

            _loadedBgUIData = backgroundUIData;
            Refresh();
        }

        public void ReselectBackgroundDropDown()
        {
            selectAssetLayout.ReselectDropDown();
        }

        void ApplyHandler()
        {
            onApply.Invoke(_loadedBgUIData);
        }
    }
}
