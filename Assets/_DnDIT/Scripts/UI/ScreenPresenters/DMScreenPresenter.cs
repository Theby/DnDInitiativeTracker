using System;
using System.Collections;
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
        readonly CharacterUIData _editableCharacterUIData = new();

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

        void ShowCreateCharacterPopup()
        {
            createCharacterPopup.SetData(_editableCharacterUIData, _data);
            createCharacterPopup.Show();
        }

        void ShowEditCharacterPopup()
        {
            editCharacterPopup.SetData(_editableCharacterUIData, _data);
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
            _data.CurrentEncounter = dataManager.CurrentEncounter;
            _data.CurrentBackground = dataManager.CurrentBackground;
            _data.CharacterNames = dataManager.GetAllCharacterNames();
            _data.BackgroundNames = dataManager.GetAllBackgroundNames();
            _data.AudioNames = dataManager.GetAllAudioNames();
        }

        void RefreshBackground()
        {
            background.texture = dataManager.CurrentBackground.Data;
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

        void AddCharacterToEncounter()
        {
            dmScreen.AddCharacterInitiativeLayout();
        }

        void RefreshEncounterOrder()
        {
            dmScreen.RefreshCharacterInitiativeLayoutList();

            //TODO Maybe Encounter, Audio and Avatar should be its own SQL Class like Background
            var updatedEncounter = dmScreen.GetEncounter();
            dataManager.UpdateEncounter(updatedEncounter);
            RefreshData();
        }

        IEnumerator CharacterEncounterSelected(int layoutIndex, string characterName)
        {
            //TODO maybe this UI and layouts should only use the name and initaitive
            //in a simple data structure and you get the proper data later when saving or going
            //back to PlayerScreen
            yield return dataManager.GetCharacterFromDataBase(characterName, characterUIData =>
            {
                dmScreen.RefreshLayout(layoutIndex, characterUIData);

                var updatedEncounter = dmScreen.GetEncounter();
                dataManager.UpdateEncounter(updatedEncounter);
                RefreshData();
            });
        }

        void RemoveCharacterFromEncounter(int positionIndex)
        {
            dmScreen.RemoveCharacterInitiativeLayout(positionIndex);

            var updatedEncounter = dmScreen.GetEncounter();
            dataManager.UpdateEncounter(updatedEncounter);
            RefreshData();
        }

        void ChangeEditableCharacterAvatar()
        {
            dataManager.GetTextureFromGallery(MediaAssetType.Avatar, textureUIData =>
            {
                if (_editableCharacterUIData.Avatar.Data != null)
                {
                    Destroy(_editableCharacterUIData.Avatar.Data);
                }

                _editableCharacterUIData.Avatar = textureUIData;

                Refresh();
            });
        }

        void ChangeEditableCharacterName(string newName)
        {
            _editableCharacterUIData.Name = newName;
        }

        void FetchAudioClipFromGallery(int index)
        {
            dataManager.GetAudioClipFromGallery(audioUIData =>
            {
                UpdateEditableCharacterUIAudioData(index, audioUIData);
            });
        }

        IEnumerator UpdateEditableCharacterAudioClip(int index, string audioName)
        {
            var currentAudioClip = _editableCharacterUIData.AudioClips.ElementAtOrDefault(index);
            if (currentAudioClip != null && currentAudioClip.Name == audioName)
                yield break;

            yield return dataManager.GetAudioClipFromDataBase(audioName, audioUIData =>
            {
                UpdateEditableCharacterUIAudioData(index, audioUIData);
            });
        }

        void UpdateEditableCharacterUIAudioData(int index, AudioUIData audioUIData)
        {
            var currentClip = _editableCharacterUIData.AudioClips.ElementAtOrDefault(index);
            if (currentClip != null)
            {
                currentClip.Data.UnloadAudioData();
                AudioClip.Destroy(currentClip.Data);
            }

            //TODO all this editable maybe should be moved somewhere else
            //Also maybe we need some Data that holds the AudioClip/Texture along the MediaAsset
            //consider making UIData classes work like that
            _editableCharacterUIData.AudioClips[index] = audioUIData;

            //TODO hackie, we should separate adding an Asset vs selection already added assets.
            //one is the add button with the field to show the path, the other is the dropdown
            //you use one or the other in the ui
            RefreshData();
            if (!_data.AudioNames.Contains(audioUIData.Name))
            {
                _data.AudioNames.Add(audioUIData.Name);
            }

            RefreshPopup();
        }

        void PlayAudio(int index)
        {
            var audioClip = _editableCharacterUIData.AudioClips.ElementAtOrDefault(index);
            if (audioClip != null)
            {
                audioSource.PlayOneShot(audioClip.Data);
            }
        }

        void SaveNewCharacter()
        {
            //TODO check that all required data is there before saving
            //for now we assume it is
            dataManager.CreateCharacter(_editableCharacterUIData);

            HidePopups();
            Refresh();
            //TODO should clear and destroy data after this?
        }

        IEnumerator CharacterSelected(int index)
        {
            //consider getting the name from the dropdown instead of index
            if (_data.CharacterNames.Count <= index)
                yield break;

            var characterName = _data.CharacterNames[index];
            if (_editableCharacterUIData.Name == characterName)
                yield break;

            yield return dataManager.GetCharacterFromDataBase(characterName, characterUIData =>
            {
                _editableCharacterUIData.SetData(characterUIData);
                RefreshPopup();
            });
        }

        void UpdateCharacter()
        {
            //TODO check that all required data is there before saving
            //for now we assume it is
            dataManager.UpdateCharacter(_editableCharacterUIData);

            HidePopups();
            Refresh();
            //TODO should clear and destroy data after this?
        }

        void SelectBackgroundFromIndex(int index)
        {
            //consider getting the name from the dropdown instead of index
            if (_data.BackgroundNames.Count <= index)
                return;

            var bgName = _data.BackgroundNames[index];
            if (_data.CurrentBackground.Name == bgName)
                return;

            dataManager.UpdateCurrentBackground(bgName);

            Refresh();
        }

        void AddNewBackground()
        {
            //TODO show loader and stop on complete
            dataManager.TryCreateNewBackground();

            Refresh();
        }

        void GoBack()
        {
            OnGoBack?.Invoke();
        }

        #region Inspector Handlers

        public void AddMoreButtonInspectorHandler()
        {
            AddCharacterToEncounter();
        }

        public void RefreshButtonInspectorHandler()
        {
            RefreshEncounterOrder();
        }

        public void CharacterEncounterSelectedInspectorHandler(int layoutIndex, string characterName)
        {
            StartCoroutine(CharacterEncounterSelected(layoutIndex, characterName));
        }

        public void RemoveCharacterLayoutInspectorHandler(int positionIndex)
        {
            RemoveCharacterFromEncounter(positionIndex);
        }

        public void CreateCharacterButtonInspectorHandler()
        {
            ShowCreateCharacterPopup();
        }

        public void EditCharacterButtonInspectorHandler()
        {
            ShowEditCharacterPopup();
        }

        public void ChangeBGButtonInspectorHandler()
        {
            ShowChangeBGPopup();
        }

        public void ChangeImageButtonInspectorHandler()
        {
            ChangeEditableCharacterAvatar();
        }

        public void NameChangedInspectorHandler(string newName)
        {
            ChangeEditableCharacterName(newName);
        }

        public void AddNewAudioButtonInspectorHandler(int index)
        {
            FetchAudioClipFromGallery(index);
        }

        public void AudioSelectionChangedInspectorHandler(int index, string audioName)
        {
            StartCoroutine(UpdateEditableCharacterAudioClip(index, audioName));
        }

        public void PlayAudioButtonInspectorHandler(int index)
        {
            PlayAudio(index);
        }

        public void SaveNewCharacterButtonInspectorHandler()
        {
            SaveNewCharacter();
        }

        public void CharacterSelectedInspectorHandler(int index)
        {
            StartCoroutine(CharacterSelected(index));
        }

        public void UpdateCharacterButtonInspectorHandler()
        {
            UpdateCharacter();
        }

        public void BackgroundSelectionDropdownInspectorHandler(int index)
        {
            SelectBackgroundFromIndex(index);
        }

        public void AddNewBackgroundButtonInspectorHandler()
        {
            AddNewBackground();
        }

        public void BackButtonInspectorHandler()
        {
            GoBack();
        }

        #endregion
    }
}