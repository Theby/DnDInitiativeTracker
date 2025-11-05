using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DnDInitiativeTracker.Controller;
using DnDInitiativeTracker.GameData;
using DnDInitiativeTracker.UIData;
using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class DataManager : MonoBehaviour
    {
        const int CurrentConfigID = 1;

        public bool IsLoaded { get; private set; }
        public CurrentConfigurationUIData CurrentConfigurationUIData { get; set; } //TODO maybe CanvasManager should have this and request the load

        SQLiteController _sqlController;
        int _defaultAvatarID;
        int _defaultAudio1Id;
        int _defaultAudio2Id;
        int _defaultAudio3Id;
        int _defaultBackgroundID;

        public IEnumerator Initialize()
        {
            _sqlController = new SQLiteController();
            _sqlController.Initialize();
            AddDefaults();

            yield return LoadData();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            _sqlController?.Dispose();
        }

        #region Defaults

        void AddDefaults()
        {
            AddDefaultMediaAssets();
            AddDefaultCharacter();
            AddDefaultCurrentConfiguration();
        }

        void AddDefaultMediaAssets()
        {
            if (!_sqlController.IsMediaAssetsTableEmpty())
                return;

            var defaultAvatarData = new MediaAssetData
            {
                Name = "DefaultAvatar",
                Type = MediaAssetType.Avatar,
                Path = Path.Combine(Application.streamingAssetsPath, "Textures/DefaultAvatar.jpg")
            };
            var defaultAudio1Data = new MediaAssetData
            {
                Name = "DefaultAudio1",
                Type = MediaAssetType.Audio,
                Path = Path.Combine(Application.streamingAssetsPath, "Audios/DefaultAudio1.mp3")
            };
            var defaultAudio2Data = new MediaAssetData
            {
                Name = "DefaultAudio2",
                Type = MediaAssetType.Audio,
                Path = Path.Combine(Application.streamingAssetsPath, "Audios/DefaultAudio2.mp3")
            };
            var defaultAudio3Data = new MediaAssetData
            {
                Name = "DefaultAudio3",
                Type = MediaAssetType.Audio,
                Path = Path.Combine(Application.streamingAssetsPath, "Audios/DefaultAudio3.mp3")
            };
            var defaultBackground = new MediaAssetData
            {
                Name = "DefaultBG",
                Type = MediaAssetType.Background,
                Path = Path.Combine(Application.streamingAssetsPath, "Textures/DefaultBG.jpg")
            };

            _defaultAvatarID = _sqlController.AddMediaAsset(defaultAvatarData);
            _defaultAudio1Id = _sqlController.AddMediaAsset(defaultAudio1Data);
            _defaultAudio2Id = _sqlController.AddMediaAsset(defaultAudio2Data);
            _defaultAudio3Id = _sqlController.AddMediaAsset(defaultAudio3Data);
            _defaultBackgroundID = _sqlController.AddMediaAsset(defaultBackground);
        }

        void AddDefaultCharacter()
        {
            if (!_sqlController.IsCharacterTableEmpty())
                return;

            var defaultAvatar = _sqlController.GetMediaAsset(_defaultAvatarID);
            var defaultAudios = new List<MediaAssetData>
            {
                _sqlController.GetMediaAsset(_defaultAudio1Id),
                _sqlController.GetMediaAsset(_defaultAudio2Id),
                _sqlController.GetMediaAsset(_defaultAudio3Id),
            };

            var defaultCharacter = new CharacterData
            {
                Avatar = defaultAvatar,
                Name = "????",
                AudioList = defaultAudios,
            };

            _sqlController.AddCharacter(defaultCharacter);
        }

        void AddDefaultCurrentConfiguration()
        {
            if (!_sqlController.IsCurrentConfigurationTableEmpty())
                return;

            var defaultBackground = _sqlController.GetMediaAsset(_defaultBackgroundID);
            var defaultConfig = new CurrentConfigurationData
            {
                Characters = new List<CharacterData>(),
                InitiativeList = new List<int>(),
                Background = defaultBackground,
            };
            _sqlController.AddCurrentConfiguration(defaultConfig);
        }

        #endregion

        #region Load Data

        IEnumerator LoadData()
        {
            IsLoaded = false;

            yield return LoadConfigurationData();

            IsLoaded = true;
        }

        IEnumerator LoadConfigurationData()
        {
            yield return GetCurrentConfigurationFromDataBase(CurrentConfigID, uiData =>
            {
                CurrentConfigurationUIData = uiData;
            });
        }

        #endregion

        #region Texture

        public List<string> GetAllAvatarNames()
        {
            return _sqlController.GetAllMediaTypeNames(MediaAssetType.Avatar);
        }

        public List<string> GetAllBackgroundNames()
        {
            return _sqlController.GetAllMediaTypeNames(MediaAssetType.Background);
        }

        public void GetTextureFromGallery(MediaAssetType type, Action<TextureUIData> onComplete)
        {
            NativeGalleryController.GetImagePathFromGallery(path =>
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                var textureUIData = GetTextureFromPath(fileName, path, type);
                onComplete?.Invoke(textureUIData);
            });
        }

        public TextureUIData GetTextureFromDataBase(string fileName, MediaAssetType type)
        {
            var mediaAsset = _sqlController.GetMediaAsset((int)type, fileName);
            var textureUIData = GetTextureFromPath(mediaAsset.Name, mediaAsset.Path, type);

            return textureUIData;
        }

        TextureUIData GetTextureFromPath(string fileName, string fullPath, MediaAssetType type)
        {
            var texture = NativeGalleryController.GetImageFromPath(fullPath);
            texture.name = fileName;

            var textureMediaAssetData = new MediaAssetData
            {
                Name = fileName,
                Path = fullPath,
                Type = type,
            };

            var textureUIData = new TextureUIData(textureMediaAssetData, texture);
            return textureUIData;
        }

        public void CreateTexture(TextureUIData textureUIData)
        {
            NativeGalleryController.SaveImageToGallery(textureUIData.Path, (fullPath, fileName) =>
            {
                textureUIData.Name = fileName;
                textureUIData.Path = fullPath;
                _sqlController.AddMediaAsset(textureUIData.ToMediaAssetData());
            });
        }

        #endregion

        #region Audio

        public List<string> GetAllAudioNames()
        {
            return _sqlController.GetAllMediaTypeNames(MediaAssetType.Audio);
        }

        public void GetAudioClipFromGallery(Action<AudioUIData> onComplete)
        {
            NativeGalleryController.GetAudioPathFromGallery(path =>
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                GetAudioClipFromPath(fileName, path, onComplete);
            });
        }

        public IEnumerator GetAudioClipFromDataBase(string audioName, Action<AudioUIData> onComplete)
        {
            var mediaAsset = _sqlController.GetMediaAsset((int)MediaAssetType.Audio, audioName);
            yield return GetAudioClipFromPath(mediaAsset.Name, mediaAsset.Path, audioUIData =>
            {
                onComplete?.Invoke(audioUIData);
            });
        }

        async Task<AudioUIData> GetAudioClipFromPath(string fileName, string fullPath, Action<AudioUIData> onComplete = null)
        {
            var audioClip = await NativeGalleryController.GetAudioClipFromPathAsync(fullPath);
            if (audioClip == null)
                return null;

            audioClip.name = fileName;

            var audioMediaAssetData = new MediaAssetData
            {
                Name = fileName,
                Path = fullPath,
                Type = MediaAssetType.Audio,
            };

            var audioUIData = new AudioUIData(audioMediaAssetData, audioClip);
            onComplete?.Invoke(audioUIData);
            return audioUIData;
        }

        void CreateAudio(AudioUIData audioUIData)
        {
            NativeGalleryController.SaveImageToGallery(audioUIData.Path, (fullPath, fileName) =>
            {
                audioUIData.Name = fileName;
                audioUIData.Path = fullPath;
                _sqlController.AddMediaAsset(audioUIData.ToMediaAssetData());
            });
        }

        #endregion

        #region Character

         public List<string> GetAllCharacterNames()
        {
            return _sqlController.GetAllCharactersNames();
        }

        public IEnumerator GetCharacterFromDataBase(string characterName, Action<CharacterUIData> onComplete)
        {
            var characterData = _sqlController.GetCharacterByName(characterName);
            yield return GetCharacterUIDataAsync(characterData, characterUIData =>
            {
                onComplete?.Invoke(characterUIData);
            });
        }

        async Task<CharacterUIData> GetCharacterUIDataAsync(CharacterData characterData, Action<CharacterUIData> onComplete = null)
        {
            var avatarUIData = GetTextureFromPath(characterData.Avatar.Name, characterData.Avatar.Path, characterData.Avatar.Type);
            var characterName = characterData.Name;
            var audioClips = new List<AudioUIData>();
            foreach (var audioData in characterData.AudioList)
            {
                var audioUIData = await GetAudioClipFromPath(audioData.Name, audioData.Path);
                audioClips.Add(audioUIData);
            }

            var characterUIData = new CharacterUIData
            {
                Avatar = avatarUIData,
                Name = characterName,
                AudioClips = audioClips,
            };

            onComplete?.Invoke(characterUIData);
            return characterUIData;
        }

        public void CreateCharacter(CharacterUIData characterUIData)
        {
            var characterData = characterUIData.ToCharacterData();
            _sqlController.AddCharacter(characterData);
        }

        public void UpdateCharacter(CharacterUIData characterUIData)
        {
            var characterData = characterUIData.ToCharacterData();
            _sqlController.UpdateCharacter(characterData);
        }

        #endregion

        #region CurrentConfiguration

        public IEnumerator GetCurrentConfigurationFromDataBase(int configID, Action<CurrentConfigurationUIData> onComplete)
        {
            CurrentConfigurationData config = _sqlController.GetCurrentConfigurationById(configID);

            var currentEncounter = new List<CharacterUIData>();
            foreach (var character in config.Characters)
            {
                yield return GetCharacterFromDataBase(character.Name, characterUIData =>
                {
                    currentEncounter.Add(characterUIData);
                });
            }

            var initiativeList = config.InitiativeList;

            var bgName = config.Background.Name;
            var bgPath = config.Background.Path;
            var bgType = config.Background.Type;
            var backgroundUIData = GetTextureFromPath(bgName, bgPath, bgType);

            var uiData = new CurrentConfigurationUIData
            {
                CurrentEncounter = currentEncounter,
                InitiativeList = initiativeList,
                CurrentBackground = backgroundUIData,
            };
            onComplete?.Invoke(uiData);
        }

        public void UpdateCurrentConfiguration(CurrentConfigurationUIData uiData)
        {
            var configurationData = uiData.ToCurrentConfigurationData();
            _sqlController.UpdateCurrentConfiguration(configurationData);
        }

        #endregion

        // public void UpdateEncounter(List<CharacterUIData> characterUIDataList)
        // {
        //     var characterDataList = characterUIDataList.ConvertAll(x => x.ToCharacterData());
        //     foreach (var characterData in characterDataList)
        //     {
        //         characterData.SQLId = _sqlController.GetCharacterByName(characterData.Name).SQLId;
        //     }
        //
        //     CurrentEncounter = characterUIDataList;
        //     CurrentConfiguration.Characters = characterDataList;
        //     _sqlController.UpdateCurrentConfiguration(CurrentConfiguration);
        // }
    }
}