using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.Controller;
using DnDInitiativeTracker.GameData;
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
    [SerializeField] DMScreen dmScreen;
    [SerializeField] ChangeBGPopup changeBGPopup;

    DMScreenData _data;

    public void Initialize()
    {
        dmScreen.Initialize();
        dmScreen.OnRemoveLayout += RemoveCharacterLayoutHandler;

        changeBGPopup.Initialize();
    }

    public void Show()
    {
        HidePopups();

        _data = new DMScreenData();

        //TODO I think data should be loaded before showing the screen,  this "current" is shown in player
        // screen too after all
        var currentConfig = dataManager.CurrentConfiguration;
        if (currentConfig != null)
        {
            _data.CurrentEncounter = new List<CharacterUIData>();
            foreach (var currentEncounterData in currentConfig.Characters)
            {
                var characterUIData = new CharacterUIData
                {
                    AvatarTexture = NativeGalleryController.GetImageFromPath(currentEncounterData.AvatarData.Path),
                    Name = currentEncounterData.Name,
                    //var audioClips = currentEncounterData.AudioDataList //TODO it loads in coroutine
                    Initiative = currentEncounterData.Initiative
                };

                _data.CurrentEncounter.Add(characterUIData);
            }

            _data.AllCharacterNames = dataManager.GetAllCharacters().Keys.ToList();

            var backgroundUIData = new BackgroundUIData
            {
                Name = currentConfig.Background.MediaAssetData.Name,
                BackgroundTexture = NativeGalleryController.GetImageFromPath(currentConfig.Background.MediaAssetData.Path)
            };
            _data.CurrentBackground = backgroundUIData;
        }

        _data.DefaultCharacter = new CharacterUIData
        {
            AvatarTexture = Resources.Load<Texture2D>(dataManager.DefaultCharacter.AvatarData.Path),
            Name = dataManager.DefaultCharacter.Name,
            AudioClips = dataManager.DefaultCharacter.AudioDataList.Select(asset => Resources.Load<AudioClip>(asset.Path)).ToList(),
            Initiative = dataManager.DefaultCharacter.Initiative
        };

        var bgNames = dataManager.GetAllBackgrounds().Keys.ToList();
        bgNames.Add(dataManager.DefaultBackground.MediaAssetData.Name);
        _data.AllBackgroundNames = bgNames;

        _data.DefaultBackground  = new BackgroundUIData
        {
            Name = dataManager.DefaultBackground.MediaAssetData.Name,
            BackgroundTexture = Resources.Load<Texture>(dataManager.DefaultBackground.MediaAssetData.Path)
        };
        _data.CurrentBackground ??= _data.DefaultBackground;
        background.texture = _data.CurrentBackground.BackgroundTexture;

        dmScreen.SetData(_data);
        dmScreen.Show();
    }

    void HidePopups()
    {
        changeBGPopup.Hide();
    }

    void ShowChangeBGPopup()
    {
        changeBGPopup.SetData(_data.CurrentBackground.Name, _data.AllBackgroundNames);
        changeBGPopup.Show();
    }

    #region Inspector Handlers

    public void CreateCharacterButtonInspectorHandler()
    {

    }

    public void EditCharacterButtonInspectorHandler()
    {

    }

    public void ChangeBGButtonInspectorHandler()
    {
        ShowChangeBGPopup();
    }

    public void AddMoreButtonInspectorHandler()
    {
        dmScreen.AddCharacterInitiativeLayout(null); //pass default character
    }

    public void RefreshButtonInspectorHandler()
    {
        dmScreen.RefreshCharacterInitiativeLayoutList();
    }

    public void BackgroundSelectionDropdownInspectorHandler(int index)
    {
        Debug.Log($"BackgroundSelectionDropdownInspectorHandler {index}");
        if (_data.AllBackgroundNames.Count <= index)
        {
            return;
        }

        var bgName = _data.AllBackgroundNames[index];
        var bgData = dataManager.GetAllBackgrounds()[bgName];
        _data.CurrentBackground = new BackgroundUIData
        {
            Name = bgName,
            BackgroundTexture = NativeGalleryController.GetImageFromPath(bgData.MediaAssetData.Path)
        };
        background.texture = _data.CurrentBackground.BackgroundTexture;
    }

    public void AddNewBackgroundButtonInspectorHandler()
    {
        NativeGalleryController.GetImageFromGallery(texture =>
        {
            _data.CurrentBackground = new BackgroundUIData
            {
                Name = texture.name,
                BackgroundTexture = texture
            };
            background.texture = _data.CurrentBackground.BackgroundTexture;
        },
            true, (fullPath, fileName) =>
            {
                var backgroundData = new BackgroundData
                {
                    MediaAssetData = new MediaAssetData
                    {
                        Name = fileName,
                        Type = NativeGallery.MediaType.Image,
                        Path = fullPath,
                    },
                };
                dataManager.AddBackground(backgroundData);
            });
    }

    #endregion

    #region Handlers

    void RemoveCharacterLayoutHandler(int positionIndex)
    {
        dmScreen.RemoveCharacterInitiativeLayout(positionIndex);
    }

    #endregion
}
