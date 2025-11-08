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
        [SerializeField] UnityEvent onAddNewAvatar;
        [SerializeField] UnityEvent<string> onAvatarSelectionChanged;
        [SerializeField] UnityEvent onAvatarRemoved;
        [SerializeField] UnityEvent onAddAudioLayout;
        [SerializeField] UnityEvent<int> onAudioLayoutRemoved;
        [SerializeField] UnityEvent<int> onAddNewAudio;
        [SerializeField] UnityEvent<int, string> onAudioSelectionChanged;
        [SerializeField] UnityEvent<int> onAudioRemovedChanged;
        [SerializeField] UnityEvent<AudioClip> onPlayAudio;
        [SerializeField] UnityEvent<CharacterUIData> onSave;

        public override void Initialize()
        {
            editCharacterLayout.Initialize();

            closeButton.onClick.AddListener(onClose.Invoke);
            editCharacterLayout.OnAddNewAvatar += onAddNewAvatar.Invoke;
            editCharacterLayout.OnAvatarSelectionChanged += onAvatarSelectionChanged.Invoke;
            editCharacterLayout.OnAvatarRemoved += onAvatarRemoved.Invoke;
            editCharacterLayout.OnAddAudioLayout += onAddAudioLayout.Invoke;
            editCharacterLayout.OnAudioLayoutRemoved += onAudioLayoutRemoved.Invoke;
            editCharacterLayout.OnAddNewAudio += onAddNewAudio.Invoke;
            editCharacterLayout.OnAudioSelectionChanged += onAudioSelectionChanged.Invoke;
            editCharacterLayout.OnAudioRemoved += onAudioRemovedChanged.Invoke;
            editCharacterLayout.OnPlayAudio += onPlayAudio.Invoke;
            saveButton.onClick.AddListener(SaveCharacterHandler);
        }

        public void SetData(DMScreenData data, CharacterUIData defaultCharacter)
        {
            editCharacterLayout.SetData(data, defaultCharacter);
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

        void SaveCharacterHandler()
        {
            var newCharacter = editCharacterLayout.EditCharacter;
            onSave.Invoke(newCharacter);
        }
    }
}