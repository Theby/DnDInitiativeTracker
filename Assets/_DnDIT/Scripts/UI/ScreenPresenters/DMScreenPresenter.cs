using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DnDInitiativeTracker.GameData;
using DnDInitiativeTracker.Manager;
using DnDInitiativeTracker.UI;
using DnDInitiativeTracker.UIData;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DnDInitiativeTracker.ScreenManager
{
    public class DMScreenPresenter : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] DataManager dataManager;
        [Header("UI")]
        [SerializeField] RawImage background;
        [SerializeField] AudioSource audioSource;
        [SerializeField] DMScreen dmScreen;
        [SerializeField] CreateCharacterPopup createCharacterPopup;
        [SerializeField] EditCharacterPopup editCharacterPopup;
        [SerializeField] ChangeBGPopup changeBGPopup;

        public event Action OnGoBack;

        readonly DMScreenData _data = new();

        public void Initialize()
        {
            dmScreen.Initialize();

            createCharacterPopup.Initialize();
            editCharacterPopup.Initialize();
            changeBGPopup.Initialize();
        }

        public void Show()
        {
            HidePopups();
            RefreshData();
            ShowDMScreen();
        }

        void ShowDMScreen()
        {
            dmScreen.SetData(_data);
            dmScreen.Show();
        }

        async Task ShowCreateCharacterPopupAsync()
        {
            var defaultCharacter = await dataManager.GetDefaultCharacterAsync();

            createCharacterPopup.SetData(_data, defaultCharacter);
            createCharacterPopup.Show();
        }

        async Task ShowEditCharacterPopupAsync()
        {
            var defaultCharacter = await dataManager.GetDefaultCharacterAsync();

            editCharacterPopup.SetData(_data, defaultCharacter);
            editCharacterPopup.Show();
        }

        void ShowChangeBGPopup()
        {
            changeBGPopup.SetData(_data);
            changeBGPopup.Show();
        }

        public void Hide()
        {
            HidePopups();

            dmScreen.Hide();
        }

        void HidePopups()
        {
            createCharacterPopup.Hide();
            editCharacterPopup.Hide();
            changeBGPopup.Hide();
        }

        void Refresh()
        {
            RefreshData();
            RefreshBackground();
            RefreshScreen();
            RefreshPopup();
        }

        void RefreshData()
        {
            _data.CurrentConfigurationUIData = dataManager.CurrentConfigurationUIData;
            _data.CharacterNames = dataManager.GetAllCharacterNames();
            _data.AvatarNames = dataManager.GetAllAvatarNames();
            _data.BackgroundNames = dataManager.GetAllBackgroundNames();
            _data.AudioNames = dataManager.GetAllAudioNames();
        }

        void RefreshBackground()
        {
            background.texture = dataManager.CurrentConfigurationUIData.CurrentBackground.Data;
        }

        void RefreshScreen()
        {
            dmScreen.Refresh();
        }

        void RefreshPopup()
        {
            if (createCharacterPopup.gameObject.activeSelf)
            {
                createCharacterPopup.Refresh();
            }

            if (editCharacterPopup.gameObject.activeSelf)
            {
                editCharacterPopup.Refresh();
            }

            if (changeBGPopup.gameObject.activeSelf)
            {
                changeBGPopup.Refresh();
            }
        }

        void GoBack()
        {
            UpdateCurrentConfigurationEncounter(dmScreen.GetEncounter());

            OnGoBack?.Invoke();
        }

        #region Encounter

        async Task AddCharacterToEncounterAsync()
        {
            var defaultCharacter = await dataManager.GetDefaultCharacterAsync();
            dmScreen.AddCharacterInitiativeLayout(defaultCharacter, 0);
        }

        void RemoveCharacterFromEncounter(int positionIndex)
        {
            dmScreen.RemoveCharacterInitiativeLayout(positionIndex);
        }

        async Task CharacterEncounterSelectedAsync(int layoutIndex, string characterName)
        {
            var characterUIData = await dataManager.GetCharacterFromDataBaseAsync(characterName);
            dmScreen.UpdateCharacter(layoutIndex, characterUIData);
        }

        void RefreshEncounterOrder()
        {
            dmScreen.RefreshEncounterOrder();
        }

        void UpdateCurrentConfigurationEncounter((List<CharacterUIData>, List<int>) encounter)
        {
            var (characterList, initiativeList) = encounter;

            _data.CurrentConfigurationUIData.CurrentEncounter = characterList;
            _data.CurrentConfigurationUIData.InitiativeList = initiativeList;
            dataManager.UpdateCurrentConfiguration(_data.CurrentConfigurationUIData);

            Refresh();
        }

        #endregion

        //TODO between Create and Edit there are a lot of common methods and copied code

        #region Create Character

        void AddNewAvatar()
        {
            dataManager.GetTextureFromGallery(MediaAssetType.Avatar, textureUIData =>
            {
                createCharacterPopup.ShowNewAvatar(textureUIData);
            });
        }

        async Task SelectAvatarAsync(string avatarName)
        {
            var previewTexture = await dataManager.GetTextureFromDataBaseAsync(avatarName, MediaAssetType.Avatar);
            createCharacterPopup.ShowAvatarDropdown(previewTexture);
        }

        void RemoveNewAvatar()
        {
            createCharacterPopup.ReselectAvatarDropDown();
        }

        async Task AddAudioLayoutAsync()
        {
            var audioUIData = await dataManager.GetDefaultAudioAsync();
            createCharacterPopup.AddAudioLayout(audioUIData);
        }

        void RemoveAudioLayout(int index)
        {
            createCharacterPopup.RemoveAudioLayout(index);
        }

        void AddNewAudio(int index)
        {
            dataManager.GetAudioClipFromGallery(audioUIData =>
            {
                createCharacterPopup.ShowNewAudio(index, audioUIData);
            });
        }

        async Task SelectAudioAsync(int index, string audioName)
        {
            var audioUIData = await dataManager.GetAudioClipFromDataBaseAsync(audioName);
            createCharacterPopup.ShowAudioDropdown(index, audioUIData);
        }

        void RemoveNewAudio(int index)
        {
            createCharacterPopup.ReselectAudioDropDown(index);
        }

        void PlayAudio(AudioClip audioClip)
        {
            audioSource.PlayOneShot(audioClip);
        }

        async Task SaveNewCharacterAsync(CharacterUIData newCharacterUIData)
        {
            if (string.IsNullOrEmpty(newCharacterUIData.Name) || dataManager.IsCharacterInDatabase(newCharacterUIData))
                return;

            createCharacterPopup.Hide();
            await dataManager.CreateCharacterAsync(newCharacterUIData);

            Refresh();
        }

        #endregion

        #region Edit Character

        async Task CharacterSelectedAsync(string characterName)
        {
            var characterUIData = await dataManager.GetCharacterFromDataBaseAsync(characterName);
            editCharacterPopup.UpdateEditCharacter(characterUIData);
        }

        void AddNewAvatarEdit()
        {
            dataManager.GetTextureFromGallery(MediaAssetType.Avatar, textureUIData =>
            {
                editCharacterPopup.ShowNewAvatar(textureUIData);
            });
        }

        async Task SelectAvatarEditAsync(string avatarName)
        {
            var previewTexture = await dataManager.GetTextureFromDataBaseAsync(avatarName, MediaAssetType.Avatar);
            editCharacterPopup.ShowAvatarDropdown(previewTexture);
        }

        void RemoveNewAvatarEdit()
        {
            editCharacterPopup.ReselectAvatarDropDown();
        }

        async Task AddAudioLayoutEditAsync()
        {
            var audioUIData = await dataManager.GetDefaultAudioAsync();
            editCharacterPopup.AddAudioLayout(audioUIData);
        }

        void RemoveAudioLayoutEdit(int index)
        {
            editCharacterPopup.RemoveAudioLayout(index);
        }

        void AddNewAudioEdit(int index)
        {
            dataManager.GetAudioClipFromGallery(audioUIData =>
            {
                editCharacterPopup.ShowNewAudio(index, audioUIData);
            });
        }

        async Task SelectAudioEditAsync(int index, string audioName)
        {
            var audioUIData = await dataManager.GetAudioClipFromDataBaseAsync(audioName);
            editCharacterPopup.ShowAudioDropdown(index, audioUIData);
        }

        void RemoveNewAudioEdit(int index)
        {
            editCharacterPopup.ReselectAudioDropDown(index);
        }

        void PlayAudioEdit(AudioClip audioClip)
        {
            audioSource.PlayOneShot(audioClip);
        }

        async Task UpdateCharacterAsync(CharacterUIData characterUIData)
        {
            if (string.IsNullOrEmpty(characterUIData.Name))
                return;

            editCharacterPopup.Hide();
            await dataManager.UpdateCharacterAsync(characterUIData);

            Refresh();
        }

        #endregion

        #region Change Background

        void AddNewBackground()
        {
            dataManager.GetTextureFromGallery(MediaAssetType.Background, textureUIData =>
            {
               changeBGPopup.ShowNewBackground(textureUIData);
            });
        }

        async Task SelectBackgroundAsync(string bgName)
        {
            var previewTexture = await dataManager.GetTextureFromDataBaseAsync(bgName, MediaAssetType.Background);
            changeBGPopup.ShowDropDown(previewTexture);
        }

        void RemoveNewBackground()
        {
            changeBGPopup.ReselectBackgroundDropDown();
        }

        void ApplyBackground(TextureUIData backgroundUIData)
        {
            changeBGPopup.Hide();

            dataManager.CreateTexture(backgroundUIData, UpdateCurrentConfigurationBackground);
        }

        void UpdateCurrentConfigurationBackground(TextureUIData backgroundUIData)
        {
            _data.CurrentConfigurationUIData.CurrentBackground = backgroundUIData;
            dataManager.UpdateCurrentConfiguration(_data.CurrentConfigurationUIData);

            Refresh();
        }

        #endregion

        #region Batch Loading

        async Task AddFromTextBatch()
        {
            await dataManager.CreateCharactersFromBatch();

            Refresh();
        }

        #endregion

        #region Main Inspector Handlers

        public void CreateCharacterButtonInspectorHandler()
        {
            ShowCreateCharacterPopupAsync();
        }

        public void EditCharacterButtonInspectorHandler()
        {
            ShowEditCharacterPopupAsync();
        }

        public void ChangeBGButtonInspectorHandler()
        {
            ShowChangeBGPopup();
        }

        public void AddTextBatchButtonInspectorHandler()
        {
            AddFromTextBatch();
        }

        public void AddMoreButtonInspectorHandler()
        {
            AddCharacterToEncounterAsync();
        }

        public void RefreshButtonInspectorHandler()
        {
            RefreshEncounterOrder();
        }

        public void BackButtonInspectorHandler()
        {
            GoBack();
        }

        #endregion

        #region Encounter Inspector Handlers

        public void CharacterEncounterSelectedInspectorHandler(int layoutIndex, string characterName)
        {
            CharacterEncounterSelectedAsync(layoutIndex, characterName);
        }

        public void RemoveCharacterLayoutInspectorHandler(int index)
        {
            RemoveCharacterFromEncounter(index);
        }

        #endregion

        #region CreateCharacterPopup Inspector Handlers

        public void AddNewAvatarInspectorHandler()
        {
            AddNewAvatar();
        }

        public void AvatarSelectionChangedInspectorHandler(string avatarName)
        {
            SelectAvatarAsync(avatarName);
        }

        public void AvatarRemovedInspectorHandler()
        {
            RemoveNewAvatar();
        }

        public void AddAudioLayoutButtonInspectorHandler()
        {
            AddAudioLayoutAsync();
        }

        public void RemoveAudioLayoutButtonInspectorHandler(int index)
        {
            RemoveAudioLayout(index);
        }

        public void AddNewAudioButtonInspectorHandler(int index)
        {
            AddNewAudio(index);
        }

        public void AudioSelectionChangedInspectorHandler(int index, string audioName)
        {
            SelectAudioAsync(index, audioName);
        }

        public void AudioRemovedInspectorHandler(int index)
        {
            RemoveNewAudio(index);
        }

        public void PlayAudioButtonInspectorHandler(AudioClip audioClip)
        {
            PlayAudio(audioClip);
        }

        public void SaveNewCharacterButtonInspectorHandler(CharacterUIData newCharacterUIData)
        {
            SaveNewCharacterAsync(newCharacterUIData);
        }

        #endregion

        #region EditCharacterPopup Inspector Handlers

        public void CharacterSelectedInspectorHandler(string characterName)
        {
            CharacterSelectedAsync(characterName);
        }

        public void AddNewAvatarInspectorHandlerEdit()
        {
            AddNewAvatarEdit();
        }

        public void AvatarSelectionChangedInspectorHandlerEdit(string avatarName)
        {
            SelectAvatarEditAsync(avatarName);
        }

        public void AvatarRemovedInspectorHandlerEdit()
        {
            RemoveNewAvatarEdit();
        }

        public void AddAudioLayoutButtonInspectorHandlerEdit()
        {
            AddAudioLayoutEditAsync();
        }

        public void RemoveAudioLayoutButtonInspectorHandlerEdit(int index)
        {
            RemoveAudioLayoutEdit(index);
        }

        public void AddNewAudioButtonInspectorHandlerEdit(int index)
        {
            AddNewAudioEdit(index);
        }

        public void AudioSelectionChangedInspectorHandlerEdit(int index, string audioName)
        {
            SelectAudioEditAsync(index, audioName);
        }

        public void AudioRemovedInspectorHandlerEdit(int index)
        {
            RemoveNewAudioEdit(index);
        }

        public void PlayAudioButtonInspectorHandlerEdit(AudioClip audioClip)
        {
            PlayAudioEdit(audioClip);
        }

        public void UpdateCharacterButtonInspectorHandler(CharacterUIData characterUIData)
        {
            UpdateCharacterAsync(characterUIData);
        }

        #endregion

        #region ChangeBGPopup Inspector Handlers

        public void AddNewBackgroundButtonInspectorHandler()
        {
            AddNewBackground();
        }

        public void BackgroundSelectionDropdownInspectorHandler(string bgName)
        {
            SelectBackgroundAsync(bgName);
        }

        public void RemoveNewBackgroundButtonInspectorHandler()
        {
            RemoveNewBackground();
        }

        public void ApplyBackgroundButtonInspectorHandler(TextureUIData backgroundUIData)
        {
            ApplyBackground(backgroundUIData);
        }

        #endregion
    }
}