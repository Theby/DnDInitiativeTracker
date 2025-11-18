using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.GameData;
using DnDInitiativeTracker.Manager;
using DnDInitiativeTracker.UI;
using DnDInitiativeTracker.UIData;
using UnityEngine;
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

        IEnumerator ShowCreateCharacterPopup()
        {
            yield return dataManager.GetDefaultCharacter(defaultCharacter =>
            {
                createCharacterPopup.SetData(_data, defaultCharacter);
                createCharacterPopup.Show();
            });
        }

        IEnumerator ShowEditCharacterPopup()
        {
            yield return dataManager.GetDefaultCharacter(defaultCharacter =>
            {
                editCharacterPopup.SetData(_data, defaultCharacter);
                editCharacterPopup.Show();
            });
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

        IEnumerator AddCharacterToEncounter()
        {
            yield return dataManager.GetDefaultCharacter(defaultCharacter =>
            {
                dmScreen.AddCharacterInitiativeLayout(defaultCharacter, 0);
            });
        }

        void RemoveCharacterFromEncounter(int positionIndex)
        {
            dmScreen.RemoveCharacterInitiativeLayout(positionIndex);
        }

        IEnumerator CharacterEncounterSelected(int layoutIndex, string characterName)
        {
            yield return dataManager.GetCharacterFromDataBase(characterName, characterUIData =>
            {
                dmScreen.UpdateCharacter(layoutIndex, characterUIData);
            });
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

        IEnumerator SelectAvatar(string avatarName)
        {
            yield return dataManager.GetTextureFromDataBase(avatarName, MediaAssetType.Avatar, previewTexture =>
            {
                createCharacterPopup.ShowAvatarDropdown(previewTexture);
            });
        }

        void RemoveNewAvatar()
        {
            createCharacterPopup.ReselectAvatarDropDown();
        }

        IEnumerator AddAudioLayout()
        {
            yield return dataManager.GetDefaultAudio(audioUIData =>
            {
                createCharacterPopup.AddAudioLayout(audioUIData);
            });
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

        IEnumerator SelectAudio(int index, string audioName)
        {
            yield return dataManager.GetAudioClipFromDataBase(audioName, audioUIData =>
            {
                createCharacterPopup.ShowAudioDropdown(index, audioUIData);
            });
        }

        void RemoveNewAudio(int index)
        {
            createCharacterPopup.ReselectAudioDropDown(index);
        }

        void PlayAudio(AudioClip audioClip)
        {
            audioSource.PlayOneShot(audioClip);
        }

        void SaveNewCharacter(CharacterUIData newCharacterUIData)
        {
            if (string.IsNullOrEmpty(newCharacterUIData.Name) || dataManager.IsCharacterInDatabase(newCharacterUIData))
                return;

            createCharacterPopup.Hide();
            StartCoroutine(dataManager.CreateCharacter(newCharacterUIData, Refresh));
        }

        #endregion

        #region Edit Character

        IEnumerator CharacterSelected(string characterName)
        {
            yield return dataManager.GetCharacterFromDataBase(characterName, characterUIData =>
            {
                editCharacterPopup.UpdateEditCharacter(characterUIData);
            });
        }

        void AddNewAvatarEdit()
        {
            dataManager.GetTextureFromGallery(MediaAssetType.Avatar, textureUIData =>
            {
                editCharacterPopup.ShowNewAvatar(textureUIData);
            });
        }

        IEnumerator SelectAvatarEdit(string avatarName)
        {
            yield return dataManager.GetTextureFromDataBase(avatarName, MediaAssetType.Avatar, previewTexture =>
            {
                editCharacterPopup.ShowAvatarDropdown(previewTexture);
            });
        }

        void RemoveNewAvatarEdit()
        {
            editCharacterPopup.ReselectAvatarDropDown();
        }

        IEnumerator AddAudioLayoutEdit()
        {
            yield return dataManager.GetDefaultAudio(audioUIData =>
            {
                editCharacterPopup.AddAudioLayout(audioUIData);
            });
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

        IEnumerator SelectAudioEdit(int index, string audioName)
        {
            yield return dataManager.GetAudioClipFromDataBase(audioName, audioUIData =>
            {
                editCharacterPopup.ShowAudioDropdown(index, audioUIData);
            });
        }

        void RemoveNewAudioEdit(int index)
        {
            editCharacterPopup.ReselectAudioDropDown(index);
        }

        void PlayAudioEdit(AudioClip audioClip)
        {
            audioSource.PlayOneShot(audioClip);
        }

        void SaveNewCharacterEdit(CharacterUIData newCharacterUIData)
        {
            if (string.IsNullOrEmpty(newCharacterUIData.Name) || dataManager.IsCharacterInDatabase(newCharacterUIData))
                return;

            editCharacterPopup.Hide();
            StartCoroutine(dataManager.CreateCharacter(newCharacterUIData, Refresh));
        }

        void UpdateCharacter(CharacterUIData characterUIData)
        {
            if (string.IsNullOrEmpty(characterUIData.Name))
                return;

            editCharacterPopup.Hide();
            StartCoroutine(dataManager.UpdateCharacter(characterUIData, Refresh));
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

        IEnumerator SelectBackground(string bgName)
        {
            yield return dataManager.GetTextureFromDataBase(bgName, MediaAssetType.Background, previewTexture =>
            {
                changeBGPopup.ShowDropDown(previewTexture);
            });
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

        #region Main Inspector Handlers

        public void CreateCharacterButtonInspectorHandler()
        {
            StartCoroutine(ShowCreateCharacterPopup());
        }

        public void EditCharacterButtonInspectorHandler()
        {
            StartCoroutine(ShowEditCharacterPopup());
        }

        public void ChangeBGButtonInspectorHandler()
        {
            ShowChangeBGPopup();
        }

        public void AddMoreButtonInspectorHandler()
        {
            StartCoroutine(AddCharacterToEncounter());
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
            StartCoroutine(CharacterEncounterSelected(layoutIndex, characterName));
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
            StartCoroutine(SelectAvatar(avatarName));
        }

        public void AvatarRemovedInspectorHandler()
        {
            RemoveNewAvatar();
        }

        public void AddAudioLayoutButtonInspectorHandler()
        {
            StartCoroutine(AddAudioLayout());
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
            StartCoroutine(SelectAudio(index, audioName));
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
            SaveNewCharacter(newCharacterUIData);
        }

        #endregion

        #region EditCharacterPopup Inspector Handlers

        public void CharacterSelectedInspectorHandler(string characterName)
        {
            StartCoroutine(CharacterSelected(characterName));
        }

        public void AddNewAvatarInspectorHandlerEdit()
        {
            AddNewAvatarEdit();
        }

        public void AvatarSelectionChangedInspectorHandlerEdit(string avatarName)
        {
            StartCoroutine(SelectAvatarEdit(avatarName));
        }

        public void AvatarRemovedInspectorHandlerEdit()
        {
            RemoveNewAvatarEdit();
        }

        public void AddAudioLayoutButtonInspectorHandlerEdit()
        {
            StartCoroutine(AddAudioLayoutEdit());
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
            StartCoroutine(SelectAudioEdit(index, audioName));
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
            UpdateCharacter(characterUIData);
        }

        #endregion

        #region ChangeBGPopup Inspector Handlers

        public void AddNewBackgroundButtonInspectorHandler()
        {
            AddNewBackground();
        }

        public void BackgroundSelectionDropdownInspectorHandler(string bgName)
        {
            StartCoroutine(SelectBackground(bgName));
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