using System;
using System.Collections.Generic;
using DnDInitiativeTracker.GameData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class ChangeBGPopup : Panel
    {
        [SerializeField] Button closeButton;
        [SerializeField] TMP_Dropdown nameDropdown;
        [SerializeField] Button addNewButton;

        public event Action OnAddNewBackground;

        public override void Initialize()
        {
            closeButton.onClick.AddListener(Hide);
            addNewButton.onClick.AddListener(OnAddNewBackgroundHandler);
        }

        public void SetData(BackgroundData data, List<string> backgroundNames)
        {
            nameDropdown.AddOptions(backgroundNames);

            var dropDownIndex = data == null ? 0 : nameDropdown.options.FindIndex(x => x.text == data.MediaAssetData.Name);
            nameDropdown.value = dropDownIndex;
        }

        void OnAddNewBackgroundHandler()
        {
            OnAddNewBackground?.Invoke();
        }
    }
}
