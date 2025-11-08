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
        [SerializeField] UnityEvent<string> onCharacterSelectionChanged;
        [SerializeField] UnityEvent onAddNewAvatar;
        [SerializeField] UnityEvent<string> onAvatarSelectionChanged;
        [SerializeField] UnityEvent onAvatarRemoved;
        [SerializeField] UnityEvent onAddAudioLayout;
        [SerializeField] UnityEvent<int> onAudioLayoutRemoved;
        [SerializeField] UnityEvent<int> onAddNewAudio;
        [SerializeField] UnityEvent<int, string> onAudioSelectionChanged;
        [SerializeField] UnityEvent<int> onAudioRemovedChanged;
        [SerializeField] UnityEvent<AudioClip> onPlayAudio;
        [SerializeField] UnityEvent<CharacterUIData> onUpdate;

        DMScreenData _data;

        public override void Initialize()
        {
            editCharacterLayout.Initialize();

            closeButton.onClick.AddListener(onClose.Invoke);
            selectCharacterDropdown.onValueChanged.AddListener(CharacterSelectionChangedHandler);
            editCharacterLayout.OnAddNewAvatar += onAddNewAvatar.Invoke;
            editCharacterLayout.OnAvatarSelectionChanged += onAvatarSelectionChanged.Invoke;
            editCharacterLayout.OnAvatarRemoved += onAvatarRemoved.Invoke;
            editCharacterLayout.OnAddAudioLayout += onAddAudioLayout.Invoke;
            editCharacterLayout.OnAudioLayoutRemoved += onAudioLayoutRemoved.Invoke;
            editCharacterLayout.OnAddNewAudio += onAddNewAudio.Invoke;
            editCharacterLayout.OnAudioSelectionChanged += onAudioSelectionChanged.Invoke;
            editCharacterLayout.OnAudioRemoved += onAudioRemovedChanged.Invoke;
            editCharacterLayout.OnPlayAudio += onPlayAudio.Invoke;
            updateButton.onClick.AddListener(UpdateCharacterHandler);
        }

        public void SetData(DMScreenData data, CharacterUIData editCharacter)
        {
            _data = data;

            selectCharacterDropdown.ClearOptions();
            selectCharacterDropdown.AddOptions(data.CharacterNames);

            var dropDownIndex = selectCharacterDropdown.options.FindIndex(x => x.text == editCharacter.Name);
            selectCharacterDropdown.SetValueWithoutNotify(dropDownIndex);

           editCharacterLayout.SetData(data, editCharacter);
        }

        public override void Show()
        {
            base.Show();

            editCharacterLayout.Show();
            Refresh();
        }

        public void Refresh()
        {
            editCharacterLayout.Refresh();
        }

        void CharacterSelectionChangedHandler(int index)
        {
            var assetName = selectCharacterDropdown.options[index].text;
            onCharacterSelectionChanged?.Invoke(assetName);
        }

        public void UpdateEditCharacter(CharacterUIData characterUIData)
        {
            editCharacterLayout.SetData(_data, characterUIData);
            editCharacterLayout.Show();
            Refresh();
        }

        public void ShowNewAvatar(TextureUIData avatarUIData)
        {
            editCharacterLayout.ShowNewAvatar(avatarUIData);
        }

        public void ShowAvatarDropdown(TextureUIData avatarUIData)
        {
            editCharacterLayout.ShowAvatarDropdown(avatarUIData);
        }

        public void ReselectAvatarDropDown()
        {
            editCharacterLayout.ReselectAvatarDropDown();
        }

        public void AddAudioLayout(AudioUIData audioUIData)
        {
            editCharacterLayout.AddAudioLayout(audioUIData);
        }

        public void RemoveAudioLayout(int index)
        {
            editCharacterLayout.RemoveAudioLayout(index);
        }

        public void ShowNewAudio(int index, AudioUIData audioUIData)
        {
            editCharacterLayout.ShowNewAudio(index, audioUIData);
        }

        public void ShowAudioDropdown(int index, AudioUIData audioUIData)
        {
            editCharacterLayout.ShowAudioDropdown(index, audioUIData);
        }

        public void ReselectAudioDropDown(int index)
        {
            editCharacterLayout.ReselectAudioDropDown(index);
        }

        void UpdateCharacterHandler()
        {
            var updateCharacter = editCharacterLayout.EditCharacter;
            onUpdate.Invoke(updateCharacter);
        }
    }
}