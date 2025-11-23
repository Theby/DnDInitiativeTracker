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
        public const string StreamingAssetTag = "#SA/";

        const int CurrentConfigID = 1;
        const int DefaultCharacterID = 1;
        const int DefaultAudioID = 2;

        public bool IsLoaded { get; private set; }
        public CurrentConfigurationUIData CurrentConfigurationUIData { get; set; } //TODO maybe CanvasManager should have this and request the load

        SQLiteController _sqlController;
        MediaAssetData _defaultAvatarData;
        MediaAssetData _defaultAudio1Data;
        MediaAssetData _defaultAudio2Data;
        MediaAssetData _defaultAudio3Data;
        MediaAssetData _defaultBackgroundData;

        readonly Dictionary<string, TextureUIData> _textureUIDataCache = new();
        readonly Dictionary<string, AudioUIData> _audioUIDataCache = new();

        public async Task InitializeAsync()
        {
            _sqlController = new SQLiteController();
            _sqlController.Initialize();
            AddDefaults();

            await LoadDataAsync();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            _sqlController?.Dispose();

            foreach (var textureUIData in _textureUIDataCache.Values)
            {
                Texture.Destroy(textureUIData.Data);
            }

            foreach (var audioUIData in _audioUIDataCache.Values)
            {
                audioUIData.Data.UnloadAudioData();
                AudioClip.Destroy(audioUIData.Data);
            }
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

            _defaultAvatarData = new MediaAssetData
            {
                Name = "DefaultAvatar",
                Type = MediaAssetType.Avatar,
                Path = $"{StreamingAssetTag}Textures/DefaultAvatar.jpg"
            };
            _defaultAudio1Data = new MediaAssetData
            {
                Name = "DefaultAudio1",
                Type = MediaAssetType.Audio,
                Path = $"{StreamingAssetTag}Audios/DefaultAudio1.mp3"
            };
            _defaultAudio2Data = new MediaAssetData
            {
                Name = "DefaultAudio2",
                Type = MediaAssetType.Audio,
                Path = $"{StreamingAssetTag}Audios/DefaultAudio2.mp3"
            };
            _defaultAudio3Data = new MediaAssetData
            {
                Name = "DefaultAudio3",
                Type = MediaAssetType.Audio,
                Path = $"{StreamingAssetTag}Audios/DefaultAudio3.mp3"
            };
            _defaultBackgroundData = new MediaAssetData
            {
                Name = "DefaultBG",
                Type = MediaAssetType.Background,
                Path = $"{StreamingAssetTag}Textures/DefaultBG.jpg"
            };

            _sqlController.AddMediaAsset(_defaultAvatarData);
            _sqlController.AddMediaAsset(_defaultAudio1Data);
            _sqlController.AddMediaAsset(_defaultAudio2Data);
            _sqlController.AddMediaAsset(_defaultAudio3Data);
            _sqlController.AddMediaAsset(_defaultBackgroundData);
        }

        void AddDefaultCharacter()
        {
            if (!_sqlController.IsCharacterTableEmpty())
                return;

            var defaultCharacter = new CharacterData
            {
                Avatar = _defaultAvatarData,
                Name = "????",
                AudioList = new List<MediaAssetData>
                {
                    _defaultAudio1Data,
                    _defaultAudio2Data,
                    _defaultAudio3Data,
                },
            };
            _sqlController.AddCharacter(defaultCharacter);
        }

        void AddDefaultCurrentConfiguration()
        {
            if (!_sqlController.IsCurrentConfigurationTableEmpty())
                return;

            var defaultConfig = new CurrentConfigurationData
            {
                Characters = new List<CharacterData>(),
                InitiativeList = new List<int>(),
                Background = _defaultBackgroundData,
            };
            _sqlController.AddCurrentConfiguration(defaultConfig);
        }

        #endregion

        #region Load Data

        async Task LoadDataAsync()
        {
            IsLoaded = false;

            await LoadConfigurationDataAsync();

            IsLoaded = true;
        }

        async Task LoadConfigurationDataAsync()
        {
            var uiData = await GetCurrentConfigurationFromDataBaseAsync(CurrentConfigID);
            CurrentConfigurationUIData = uiData;
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
                var mediaAsset = new MediaAssetData
                {
                    Name = fileName,
                    Path = path,
                    Type = type,
                };

                GetTextureFromGalleryCompletionCall(mediaAsset, onComplete);
            });
            return;

            async Task GetTextureFromGalleryCompletionCall(MediaAssetData loadedMedia, Action<TextureUIData> completionCallback)
            {
                var audioUIData = await GetTextureFromPathAsync(loadedMedia);
                completionCallback.Invoke(audioUIData);
            }
        }

        public async Task<TextureUIData> GetTextureFromDataBaseAsync(string fileName, MediaAssetType type)
        {
            var mediaAsset = _sqlController.GetMediaAsset((int)type, fileName);
            var textureUIData = await GetTextureFromPathAsync(mediaAsset);
            return textureUIData;
        }

        async Task<TextureUIData> GetTextureFromPathAsync(MediaAssetData mediaAssetData)
        {
            var cacheTexture = GetTextureFromCache(mediaAssetData.Path);
            if (cacheTexture != null)
                return cacheTexture;

            var texture = await NativeGalleryController.GetImageFromPathAsync(mediaAssetData.Path);
            if (texture == null)
                return null;

            texture.name = mediaAssetData.Name;

            var textureUIData = new TextureUIData(mediaAssetData, texture);
            AddTextureToCache(textureUIData);

            return textureUIData;
        }

        TextureUIData GetTextureFromCache(string fullPath)
        {
            TextureUIData cacheUIData = null;
            if (_textureUIDataCache.TryGetValue(fullPath, out var textureUIData))
            {
                cacheUIData = textureUIData;
            }

            return cacheUIData;
        }

        void AddTextureToCache(TextureUIData textureUIData)
        {
            if (_textureUIDataCache.TryAdd(textureUIData.Path, textureUIData))
                return;

            Debug.LogError("Failed to add texture to cache, already exists: " + textureUIData.Path);
        }

        void ReplaceTextureInCache(string oldPath, TextureUIData newTextureUIData)
        {
            if (oldPath == newTextureUIData.Path)
                return;

            _textureUIDataCache.Remove(oldPath);
            AddTextureToCache(newTextureUIData);
        }

        public void CreateTexture(TextureUIData textureUIData, Action<TextureUIData> onComplete)
        {
            if (IsTextureInDatabase(textureUIData))
            {
                onComplete?.Invoke(textureUIData);
                return;
            }

            NativeGalleryController.SaveImageToGallery(textureUIData.Path, (fullPath, fileName) =>
            {
                var oldPath = textureUIData.Path;
                textureUIData.Name = fileName;
                textureUIData.Path = fullPath;

                ReplaceTextureInCache(oldPath, textureUIData);

                var mediaAsset = textureUIData.ToMediaAssetData();
                _sqlController.AddMediaAsset(mediaAsset);

                textureUIData.UpdateMediaData(mediaAsset);

                onComplete?.Invoke(textureUIData);
            });
        }

        public bool IsTextureInDatabase(TextureUIData textureUIData)
        {
            var mediaAsset = textureUIData.ToMediaAssetData();
            return _sqlController.ExistsMediaAsset(mediaAsset.SQLId);
        }

        #endregion

        #region Audio

        public List<string> GetAllAudioNames()
        {
            return _sqlController.GetAllMediaTypeNames(MediaAssetType.Audio);
        }

        public async Task<AudioUIData> GetDefaultAudioAsync()
        {
            var mediaAsset = _sqlController.GetMediaAsset(DefaultAudioID);
            var audioUIData = await GetAudioClipFromPathAsync(mediaAsset);
            return audioUIData;
        }

        public void GetAudioClipFromGallery(Action<AudioUIData> onComplete)
        {
            NativeGalleryController.GetAudioPathFromGallery(path =>
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                var mediaAssetData = new MediaAssetData
                {
                    Name = fileName,
                    Path = path,
                    Type = MediaAssetType.Audio,
                };
                
                GetAudioClipFromGalleryCompletionCall(mediaAssetData, onComplete);
            });
            return;

            async Task GetAudioClipFromGalleryCompletionCall(MediaAssetData loadedMedia, Action<AudioUIData> completionCallback)
            {
                var audioUIData = await GetAudioClipFromPathAsync(loadedMedia);
                completionCallback.Invoke(audioUIData);
            }
        }

        public async Task<AudioUIData> GetAudioClipFromDataBaseAsync(string audioName)
        {
            var mediaAsset = _sqlController.GetMediaAsset((int)MediaAssetType.Audio, audioName);
            var audioUIData = await GetAudioClipFromPathAsync(mediaAsset);
            return audioUIData;
        }

        async Task<AudioUIData> GetAudioClipFromPathAsync(MediaAssetData mediaAssetData)
        {
            var cachedAudioClip = GetAudioFromCache(mediaAssetData.Path);
            if (cachedAudioClip != null)
            {
                return cachedAudioClip;
            }

            var audioClip = await NativeGalleryController.GetAudioClipFromPathAsync(mediaAssetData.Path);
            if (audioClip == null)
                return null;

            audioClip.name = mediaAssetData.Name;

            var audioUIData = new AudioUIData(mediaAssetData, audioClip);
            AddAudioToCache(audioUIData);

            return audioUIData;
        }

        AudioUIData GetAudioFromCache(string fullPath)
        {
            AudioUIData cacheUIData = null;
            if (_audioUIDataCache.TryGetValue(fullPath, out var audioUIData))
            {
                cacheUIData = audioUIData;
            }

            return cacheUIData;
        }

        void AddAudioToCache(AudioUIData audioUIData)
        {
            if (_audioUIDataCache.TryAdd(audioUIData.Path, audioUIData))
                return;

            Debug.LogError("Failed to add audio to cache, already exists: " + audioUIData.Path);
        }

        void ReplaceAudioInCache(string oldPath, AudioUIData audioUIData)
        {
            _audioUIDataCache.Remove(oldPath);
            _audioUIDataCache.Add(audioUIData.Path, audioUIData);
        }

        void CreateAudio(AudioUIData audioUIData, Action<AudioUIData> onComplete)
        {
            if (IsAudioInDatabase(audioUIData))
            {
                onComplete?.Invoke(audioUIData);
                return;
            }

            NativeGalleryController.SaveAudioToGallery(audioUIData.Path, (fullPath, fileName) =>
            {
                var oldPath = audioUIData.Path;
                audioUIData.Name = fileName;
                audioUIData.Path = fullPath;

                ReplaceAudioInCache(oldPath, audioUIData);

                var mediaAsset = audioUIData.ToMediaAssetData();
                _sqlController.AddMediaAsset(mediaAsset);

                audioUIData.UpdateMediaData(mediaAsset);

                onComplete?.Invoke(audioUIData);
            });
        }

        public bool IsAudioInDatabase(AudioUIData audioUIData)
        {
            var mediaAsset = audioUIData.ToMediaAssetData();
            return _sqlController.ExistsMediaAsset(mediaAsset.SQLId);
        }

        #endregion

        #region Character

        public List<string> GetAllCharacterNames()
        {
            return _sqlController.GetAllCharactersNames();
        }

        public async Task<CharacterUIData> GetDefaultCharacterAsync()
        {
            var characterData = _sqlController.GetCharacter(DefaultCharacterID);
            var characterUIData = await GetCharacterFromDataBaseAsync(characterData);
            return characterUIData;
        }

        public async Task<CharacterUIData> GetCharacterFromDataBaseAsync(string characterName)
        {
            var characterData = _sqlController.GetCharacter(characterName);
            var characterUIData = await GetCharacterFromDataBaseAsync(characterData);
            return characterUIData;
        }

        public async Task<CharacterUIData> GetCharacterFromDataBaseAsync(CharacterData characterData)
        {
            var avatarUIData = await GetTextureFromPathAsync(characterData.Avatar);
            var characterName = characterData.Name;
            var audioClips = new List<AudioUIData>();
            foreach (var audioData in characterData.AudioList)
            {
                var audioUIData = await GetAudioClipFromPathAsync(audioData);
                audioClips.Add(audioUIData);
            }

            var characterUIData = new CharacterUIData(characterData, avatarUIData, characterName, audioClips);
            return characterUIData;
        }

        public IEnumerator CreateCharacter(CharacterUIData characterUIData, Action onComplete)
        {
            yield return CreateCharacterAssets(characterUIData);

            var characterData = characterUIData.ToCharacterData();
            _sqlController.AddCharacter(characterData);

            onComplete?.Invoke();
        }

        public IEnumerator UpdateCharacter(CharacterUIData characterUIData, Action onComplete)
        {
            yield return CreateCharacterAssets(characterUIData);

            var characterData = characterUIData.ToCharacterData();
            _sqlController.UpdateCharacter(characterData);

            onComplete?.Invoke();
        }

        IEnumerator CreateCharacterAssets(CharacterUIData characterUIData)
        {
            bool textureComplete = false;
            CreateTexture(characterUIData.Avatar, _ => textureComplete = true);
            yield return new WaitUntil(() => textureComplete);

            foreach (var audioUIData in characterUIData.AudioList)
            {
                bool audioComplete = false;
                CreateAudio(audioUIData, _ => audioComplete = true);
                yield return new WaitUntil(() => audioComplete);
            }
        }

        public bool IsCharacterInDatabase(CharacterUIData characterUIData)
        {
            var characterData = _sqlController.GetCharacter(characterUIData.Name);
            return characterData != null;
        }

        #endregion

        #region CurrentConfiguration

        public async Task<CurrentConfigurationUIData> GetCurrentConfigurationFromDataBaseAsync(int configID)
        {
            CurrentConfigurationData config = _sqlController.GetCurrentConfigurationById(configID);

            var currentEncounter = new List<CharacterUIData>();
            foreach (var character in config.Characters)
            {
                var characterUIData = await GetCharacterFromDataBaseAsync(character);
                currentEncounter.Add(characterUIData);
            }

            var initiativeList = config.InitiativeList;

            var backgroundUIData = await GetTextureFromPathAsync(config.Background);
            var uiData = new CurrentConfigurationUIData(config, currentEncounter, initiativeList, backgroundUIData);

            return uiData;
        }

        public void UpdateCurrentConfiguration(CurrentConfigurationUIData uiData)
        {
            var configurationData = uiData.ToCurrentConfigurationData();
            _sqlController.UpdateCurrentConfiguration(configurationData);
        }

        #endregion
    }
}