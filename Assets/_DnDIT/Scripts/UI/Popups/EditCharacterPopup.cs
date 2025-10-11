using DnDInitiativeTracker.UIData;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class EditCharacterPopup : Panel
    {
        [Header("UI")]
        [SerializeField] Button closeButton;
        [SerializeField] TMP_Dropdown selectCharacterDropdown;
        [SerializeField] EditCharacterLayout editCharacterLayout;
        [SerializeField] Button updateButton;
        [Header("Events")]
        [SerializeField] UnityEvent onClose;
        [SerializeField] UnityEvent<int> onSelectCharacter;
        [SerializeField] UnityEvent onChangeImage;
        [SerializeField] UnityEvent<string> onNameChanged;
        [SerializeField] UnityEvent<int> onAddNewAudio;
        [SerializeField] UnityEvent<int, string> onAudioSelectionChanged;
        [SerializeField] UnityEvent<int> onPlayAudio;
        [SerializeField] UnityEvent onUpdate;

        DMScreenData _data;
        CharacterUIData _characterUIData;

        public override void Initialize()
        {
            editCharacterLayout.Initialize();

            closeButton.onClick.AddListener(onClose.Invoke);
            selectCharacterDropdown.onValueChanged.AddListener(onSelectCharacter.Invoke);
            editCharacterLayout.OnChangeImage += onChangeImage.Invoke;
            editCharacterLayout.OnNameChanged += onNameChanged.Invoke;
            editCharacterLayout.OnAudioSelectionChanged += onAudioSelectionChanged.Invoke;
            editCharacterLayout.OnAddNewAudio += onAddNewAudio.Invoke;

            editCharacterLayout.OnPlayAudio += onPlayAudio.Invoke;
            updateButton.onClick.AddListener(onUpdate.Invoke);
        }

        public void SetData(CharacterUIData characterUIData, DMScreenData data)
        {
            _data = data;
            _characterUIData = characterUIData;
        }

        public override void Show()
        {
            base.Show();

            Refresh();
        }

        public void Refresh()
        {
            selectCharacterDropdown.ClearOptions();
            selectCharacterDropdown.AddOptions(_data.CharacterNames);

            var dropDownIndex = selectCharacterDropdown.options.FindIndex(x => x.text == _characterUIData.Name);
            selectCharacterDropdown.value = dropDownIndex;

            editCharacterLayout.SetData(_characterUIData, _data.AudioNames);
        }
    }
}