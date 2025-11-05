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

        //TODO this seems to me like a CurrentConfiguration UI Data
        public List<CharacterUIData> CurrentEncounter { get; set; }
        public List<int> InitiativeList { get; set; }
        public TextureUIData CurrentBackground { get; set; }

        CurrentConfigurationData CurrentConfiguration { get; set; }

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

            LoadConfigurationData();
            yield return LoadUIData();

            IsLoaded = true;
        }

        void LoadConfigurationData()
        {
            CurrentConfigurationData config = _sqlController.GetCurrentConfigurationById(CurrentConfigID);
            CurrentConfiguration = config;
        }

        IEnumerator LoadUIData()
        {
            yield return LoadCurrentEncounterUIData();
            LoadBackgroundUIData();
        }

        IEnumerator LoadCurrentEncounterUIData()
        {
            var currentEncounter = new List<CharacterUIData>();
            foreach (var character in CurrentConfiguration.Characters)
            {
                yield return GetCharacterFromDataBase(character.Name, characterUIData =>
                {
                    currentEncounter.Add(characterUIData);
                });
            }

            CurrentEncounter = currentEncounter;
            InitiativeList = CurrentConfiguration.InitiativeList;
        }

        void LoadBackgroundUIData()
        {
            if (CurrentBackground != null)
            {
                Destroy(CurrentBackground.Data);
            }

            var bgName = CurrentConfiguration.Background.Name;
            var bgPath = CurrentConfiguration.Background.Path;
            var bgType = CurrentConfiguration.Background.Type;
            var backgroundUIData = GetTextureFromPath(bgName, bgPath, bgType);
            CurrentBackground = backgroundUIData;
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

        //TODO should we really save a copy of the image right away?
        public void GetTextureFromGallery(MediaAssetType type, Action<TextureUIData> onComplete)
        {
            NativeGalleryController.GetImagePathFromGallery(path =>
            {
                NativeGalleryController.SaveImageToGallery(path, (fullPath, fileName) =>
                {
                    var textureUIData = GetTextureFromPath(fileName, fullPath, type);
                    onComplete?.Invoke(textureUIData);
                });
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

        #endregion

        #region Audio

        public List<string> GetAllAudioNames()
        {
            return _sqlController.GetAllMediaTypeNames(MediaAssetType.Audio);
        }

        //TODO should we really save a copy of the audio right away?
        public void GetAudioClipFromGallery(Action<AudioUIData> onComplete)
        {
            NativeGalleryController.GetAudioPathFromGallery(path =>
            {
                NativeGalleryController.SaveAudioToGallery(path, (fullPath, fileName) =>
                {
                    GetAudioClipFromPath(fileName, fullPath, onComplete);
                });
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

        //TODO how to handle CurrentConfiguration? maybe same pattern as the others?
        public void UpdateEncounter(List<CharacterUIData> characterUIDataList)
        {
            var characterDataList = characterUIDataList.ConvertAll(x => x.ToCharacterData());
            foreach (var characterData in characterDataList)
            {
                //TODO UIData should have a way to get or remember this id
                characterData.SQLId = _sqlController.GetCharacterByName(characterData.Name).SQLId;
            }

            CurrentEncounter = characterUIDataList;
            CurrentConfiguration.Characters = characterDataList;
            _sqlController.UpdateCurrentConfiguration(CurrentConfiguration);
        }

        public void TryCreateNewBackground(Action onComplete = null)
        {
            NativeGalleryController.GetImagePathFromGallery(path =>
            {
                NativeGalleryController.SaveImageToGallery(path, (fullPath, fileName) =>
                {
                    CreateNewCurrentBackground(fullPath, fileName);
                    onComplete?.Invoke();
                });
            });
        }

        void CreateNewCurrentBackground(string fullPath, string fileName)
        {
            var backgroundData = new MediaAssetData
            {
                Name = fileName,
                Type = MediaAssetType.Background,
                Path = fullPath,
            };
            backgroundData.SQLId = _sqlController.AddMediaAsset(backgroundData);

            CurrentConfiguration.Background = backgroundData;
            _sqlController.UpdateCurrentConfiguration(CurrentConfiguration);
            LoadBackgroundUIData();
        }

        public void UpdateCurrentBackground(string bgName)
        {
            var backgroundData = _sqlController.GetMediaAsset((int)MediaAssetType.Background, bgName);
            CurrentConfiguration.Background = backgroundData;
            _sqlController.UpdateCurrentConfiguration(CurrentConfiguration);
            LoadBackgroundUIData();
        }
    }
}