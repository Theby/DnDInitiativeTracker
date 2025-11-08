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
                Path = Path.Combine(Application.streamingAssetsPath, "Textures/DefaultAvatar.jpg")
            };
            _defaultAudio1Data = new MediaAssetData
            {
                Name = "DefaultAudio1",
                Type = MediaAssetType.Audio,
                Path = Path.Combine(Application.streamingAssetsPath, "Audios/DefaultAudio1.mp3")
            };
            _defaultAudio2Data = new MediaAssetData
            {
                Name = "DefaultAudio2",
                Type = MediaAssetType.Audio,
                Path = Path.Combine(Application.streamingAssetsPath, "Audios/DefaultAudio2.mp3")
            };
            _defaultAudio3Data = new MediaAssetData
            {
                Name = "DefaultAudio3",
                Type = MediaAssetType.Audio,
                Path = Path.Combine(Application.streamingAssetsPath, "Audios/DefaultAudio3.mp3")
            };
            _defaultBackgroundData = new MediaAssetData
            {
                Name = "DefaultBG",
                Type = MediaAssetType.Background,
                Path = Path.Combine(Application.streamingAssetsPath, "Textures/DefaultBG.jpg")
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

                var mediaAsset = new MediaAssetData
                {
                    Name = fileName,
                    Path = path,
                    Type = type,
                };

                var textureUIData = GetTextureFromPath(mediaAsset);
                onComplete?.Invoke(textureUIData);
            });
        }

        public TextureUIData GetTextureFromDataBase(string fileName, MediaAssetType type)
        {
            var mediaAsset = _sqlController.GetMediaAsset((int)type, fileName);
            var textureUIData = GetTextureFromPath(mediaAsset);

            return textureUIData;
        }

        TextureUIData GetTextureFromPath(MediaAssetData mediaAssetData)
        {
            var cacheTexture = GetTextureFromCache(mediaAssetData.Path);
            if (cacheTexture != null)
                return cacheTexture;

            var texture = NativeGalleryController.GetImageFromPath(mediaAssetData.Path);
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

        public IEnumerator GetDefaultAudio(Action<AudioUIData> onComplete)
        {
            var mediaAsset = _sqlController.GetMediaAsset(DefaultAudioID);
            yield return GetAudioClipFromPath(mediaAsset, audioUIData =>
            {
                onComplete?.Invoke(audioUIData);
            });
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
                
                GetAudioClipFromPath(mediaAssetData, onComplete);
            });
        }

        public IEnumerator GetAudioClipFromDataBase(string audioName, Action<AudioUIData> onComplete)
        {
            var mediaAsset = _sqlController.GetMediaAsset((int)MediaAssetType.Audio, audioName);
            yield return GetAudioClipFromPath(mediaAsset, audioUIData =>
            {
                onComplete?.Invoke(audioUIData);
            });
        }

        async Task<AudioUIData> GetAudioClipFromPath(MediaAssetData mediaAssetData, Action<AudioUIData> onComplete = null)
        {
            var cachedAudioClip = GetAudioFromCache(mediaAssetData.Path);
            if (cachedAudioClip != null)
            {
                onComplete?.Invoke(cachedAudioClip);
                return cachedAudioClip;
            }

            var audioClip = await NativeGalleryController.GetAudioClipFromPathAsync(mediaAssetData.Path);
            if (audioClip == null)
                return null;

            audioClip.name = mediaAssetData.Name;

            var audioUIData = new AudioUIData(mediaAssetData, audioClip);
            AddAudioToCache(audioUIData);

            onComplete?.Invoke(audioUIData);
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

            NativeGalleryController.SaveImageToGallery(audioUIData.Path, (fullPath, fileName) =>
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

        public IEnumerator GetCharacterFromDataBase(string characterName, Action<CharacterUIData> onComplete)
        {
            var characterData = _sqlController.GetCharacter(characterName);
            yield return GetCharacterFromDataBase(characterData, characterUIData =>
            {
                onComplete?.Invoke(characterUIData);
            });
        }

        public IEnumerator GetCharacterFromDataBase(CharacterData characterData, Action<CharacterUIData> onComplete)
        {
            yield return GetCharacterUIDataAsync(characterData, characterUIData =>
            {
                onComplete?.Invoke(characterUIData);
            });
        }

        public IEnumerator GetDefaultCharacter(Action<CharacterUIData> onComplete)
        {
            var characterData = _sqlController.GetCharacter(DefaultCharacterID);
            yield return GetCharacterUIDataAsync(characterData, characterUIData =>
            {
                onComplete?.Invoke(characterUIData);
            });
        }

        async Task<CharacterUIData> GetCharacterUIDataAsync(CharacterData characterData, Action<CharacterUIData> onComplete = null)
        {
            var avatarUIData = GetTextureFromPath(characterData.Avatar);
            var characterName = characterData.Name;
            var audioClips = new List<AudioUIData>();
            foreach (var audioData in characterData.AudioList)
            {
                var audioUIData = await GetAudioClipFromPath(audioData);
                audioClips.Add(audioUIData);
            }

            var characterUIData = new CharacterUIData(characterData, avatarUIData, characterName, audioClips);
            onComplete?.Invoke(characterUIData);
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

        public IEnumerator GetCurrentConfigurationFromDataBase(int configID, Action<CurrentConfigurationUIData> onComplete)
        {
            CurrentConfigurationData config = _sqlController.GetCurrentConfigurationById(configID);

            var currentEncounter = new List<CharacterUIData>();
            foreach (var character in config.Characters)
            {
                yield return GetCharacterFromDataBase(character, characterUIData =>
                {
                    currentEncounter.Add(characterUIData);
                });
            }

            var initiativeList = config.InitiativeList;
            var backgroundUIData = GetTextureFromPath(config.Background);

            var uiData = new CurrentConfigurationUIData(config, currentEncounter, initiativeList, backgroundUIData);
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