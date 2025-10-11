using System.Linq;
using DnDInitiativeTracker.Manager;
using DnDInitiativeTracker.UI;
using DnDInitiativeTracker.UIData;
using UnityEngine;
using UnityEngine.UI;

public class DMScreenManager : MonoBehaviour
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

    readonly DMScreenData _data = new();
    readonly CharacterUIData _editableCharacterUIData = new();

    public void Initialize()
    {
        dmScreen.Initialize();
        dmScreen.OnRemoveLayout += RemoveCharacterLayoutHandler;

        createCharacterPopup.Initialize();
        editCharacterPopup.Initialize();
        changeBGPopup.Initialize();

        dataManager.OnDataUpdated += Refresh;
    }

    void OnDestroy()
    {
        _editableCharacterUIData.Dispose();
    }

    public void Show()
    {
        HidePopups();

        RefreshData();

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
        _editableCharacterUIData.Dispose();
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
        background.texture = dataManager.CurrentBackground.BackgroundTexture;
    }

    void RefreshPopup()
    {
        if(createCharacterPopup.gameObject.activeSelf)
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

    void ChangeEditableCharacterAvatar()
    {
        dataManager.GetAvatarFromGallery((fullPath ,texture) =>
        {
            if (_editableCharacterUIData.AvatarTexture  != null)
            {
                Destroy(_editableCharacterUIData.AvatarTexture);
            }

            _editableCharacterUIData.AvatarTexture = texture;
            _editableCharacterUIData.AvatarPath = fullPath;

            Refresh();
        });
    }

    void ChangeEditableCharacterName(string newName)
    {
        _editableCharacterUIData.Name = newName;
    }

    void FetchAudioClipFromGallery(int index)
    {
        dataManager.GetAudioClipFromGallery((fullPath, audioClip) =>
        {
           UpdateEditableCharacterUIAudioData(index, audioClip, fullPath);
        });
    }

    void UpdateEditableCharacterAudioClip(int index, string audioName)
    {
        var currentAudioClip = _editableCharacterUIData.AudioClips.ElementAtOrDefault(index);
        if (currentAudioClip != null && currentAudioClip.name == audioName)
            return;

        dataManager.GetAudioClipByName(audioName, (fullPath, audioClip) =>
        {
            UpdateEditableCharacterUIAudioData(index, audioClip, fullPath);
        });
    }

    void UpdateEditableCharacterUIAudioData(int index, AudioClip audioClip, string fullPath)
    {
        var currentClip = _editableCharacterUIData.AudioClips.ElementAtOrDefault(index);
        if (currentClip != null)
        {
            currentClip.UnloadAudioData();
            AudioClip.Destroy(currentClip);
        }

        //TODO all this editable maybe should be moved somewhere else
        //Also maybe we need some Data that holds the AudioClip/Texture along the MediaAsset
        //consider making UIData classes work like that
        _editableCharacterUIData.AudioClips[index] = audioClip;
        _editableCharacterUIData.AudioClipPaths[index] = fullPath;

        //TODO hackie, we should separate adding an Asset vs selection already added assets.
        //one is the add button with the field to show the path, the other is the dropdown
        //you use one or the other in the ui
        RefreshData();
        if (!_data.AudioNames.Contains(audioClip.name))
        {
            _data.AudioNames.Add(audioClip.name);
        }

        RefreshPopup();
    }

    void PlayAudio(int index)
    {
        var audioClip = _editableCharacterUIData.AudioClips.ElementAtOrDefault(index);
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
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

    void CharacterSelected(int index)
    {
        //consider getting the name from the dropdown instead of index
        if (_data.CharacterNames.Count <= index)
            return;

        var characterName = _data.CharacterNames[index];
        if (_editableCharacterUIData.Name == characterName)
            return;

        var characterData = dataManager.GetCharacterByName(characterName);
        dataManager.CreateCharacterUIData(characterData, characterUIData =>
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
    }

    void AddNewBackground()
    {
        //TODO show loader and stop on complete
        dataManager.TryCreateNewBackground();
    }

    #region Inspector Handlers

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

    public void AddMoreButtonInspectorHandler()
    {
        //dmScreen.AddCharacterInitiativeLayout(null); //pass default character
    }

    public void RefreshButtonInspectorHandler()
    {
        //dmScreen.RefreshCharacterInitiativeLayoutList();
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
        UpdateEditableCharacterAudioClip(index, audioName);
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
        CharacterSelected(index);
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

    #endregion

    #region Handlers

    void RemoveCharacterLayoutHandler(int positionIndex)
    {
        //dmScreen.RemoveCharacterInitiativeLayout(positionIndex);
    }

    #endregion
}
