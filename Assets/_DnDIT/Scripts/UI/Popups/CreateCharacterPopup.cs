using DnDInitiativeTracker.UIData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class CreateCharacterPopup : Panel
    {
        [Header("UI")]
        [SerializeField] Button closeButton;
        [SerializeField] EditCharacterLayout editCharacterLayout;
        [SerializeField] Button saveButton;
        [Header("Events")]
        [SerializeField] UnityEvent onClose;
        [SerializeField] UnityEvent onChangeImage;
        [SerializeField] UnityEvent<string> onNameChanged;
        [SerializeField] UnityEvent<int> onAddNewAudio;
        [SerializeField] UnityEvent<int, string> onAudioSelectionChanged;
        [SerializeField] UnityEvent<int> onPlayAudio;
        [SerializeField] UnityEvent onSave;

        DMScreenData _data;
        CharacterUIData _characterUIData;

        public override void Initialize()
        {
            editCharacterLayout.Initialize();

            closeButton.onClick.AddListener(onClose.Invoke);
            editCharacterLayout.OnChangeImage += onChangeImage.Invoke;
            editCharacterLayout.OnNameChanged += onNameChanged.Invoke;
            editCharacterLayout.OnAudioSelectionChanged += onAudioSelectionChanged.Invoke;
            editCharacterLayout.OnAddNewAudio += onAddNewAudio.Invoke;

            editCharacterLayout.OnPlayAudio += onPlayAudio.Invoke;
            saveButton.onClick.AddListener(onSave.Invoke);
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
            editCharacterLayout.SetData(_characterUIData, _data.AudioNames);
        }
    }
}