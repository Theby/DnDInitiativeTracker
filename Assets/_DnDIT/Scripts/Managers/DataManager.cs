using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DnDInitiativeTracker.Controller;
using DnDInitiativeTracker.Extensions;
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
        const int DefaultAvatarID = 1;
        const int DefaultAudio1ID = 2;
        const int DefaultAudio2ID = 3;
        const int DefaultAudio3ID = 4;

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

        public async Task<TextureUIData> GetDefaultAvatarAsync()
        {
            var mediaAsset = _sqlController.GetMediaAsset(DefaultAvatarID);
            var textureUIData = await GetTextureFromPathAsync(mediaAsset);
            return textureUIData;
        }

        public void GetTextureFromGallery(MediaAssetType type, Action<TextureUIData> onComplete)
        {
            NativeBrowserController.GetImagePathFromGallery(path =>
            {
                GetTextureFromGalleryCompletionCall(path, type, onComplete);
            });
            return;

            async Task GetTextureFromGalleryCompletionCall(string loadedPath, MediaAssetType loadedType, Action<TextureUIData> completionCallback)
            {
                var audioUIData = await GetTextureFromPathAsync(loadedPath, loadedType);
                completionCallback.Invoke(audioUIData);
            }
        }

        public async Task<TextureUIData> GetTextureFromDataBaseAsync(string fileName, MediaAssetType type)
        {
            var mediaAsset = _sqlController.GetMediaAsset((int)type, fileName);
            var textureUIData = await GetTextureFromPathAsync(mediaAsset);
            return textureUIData;
        }

        public async Task<TextureUIData> GetTextureFromDataBaseAsync(string path)
        {
            var mediaAsset = _sqlController.GetMediaAsset(path);
            var textureUIData = await GetTextureFromPathAsync(mediaAsset);
            return textureUIData;
        }

        async Task<TextureUIData> GetTextureFromPathAsync(string path, MediaAssetType type)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var mediaAsset = new MediaAssetData
            {
                Name = fileName,
                Path = path,
                Type = type,
            };

            var textureUIData = await GetTextureFromPathAsync(mediaAsset);
            return textureUIData;
        }

        async Task<TextureUIData> GetTextureFromPathAsync(MediaAssetData mediaAssetData)
        {
            var cacheTexture = GetTextureFromCache(mediaAssetData.Path);
            if (cacheTexture != null)
                return cacheTexture;

            var texture = await NativeBrowserController.GetImageFromPathAsync(mediaAssetData.Path);
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

            NativeBrowserController.SaveImageToGallery(textureUIData.Path, (fullPath, fileName) =>
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    onComplete?.Invoke(textureUIData);
                    return;
                }

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

        public bool IsTextureInDatabase(string path)
        {
            return _sqlController.ExistsMediaAsset(path);
        }

        #endregion

        #region Audio

        public List<string> GetAllAudioNames()
        {
            return _sqlController.GetAllMediaTypeNames(MediaAssetType.Audio);
        }

        public async Task<AudioUIData> GetDefaultAudioAsync()
        {
            var mediaAsset = _sqlController.GetMediaAsset(DefaultAudio1ID);
            var audioUIData = await GetAudioClipFromPathAsync(mediaAsset);
            return audioUIData;
        }

        public async Task<List<AudioUIData>> GetAllAudioDefaultsAsync()
        {
            var defaultAudios = new List<AudioUIData>();

            var defaultIds = new List<int> { DefaultAudio1ID, DefaultAudio2ID, DefaultAudio3ID };
            foreach (var defaultId in defaultIds)
            {
                var mediaAsset = _sqlController.GetMediaAsset(defaultId);
                var audioUIData = await GetAudioClipFromPathAsync(mediaAsset);

                defaultAudios.Add(audioUIData);
            }

            return defaultAudios;
        }

        public void GetAudioClipFromGallery(Action<AudioUIData> onComplete)
        {
            NativeBrowserController.GetAudioPathFromGallery(path =>
            {
                GetAudioClipFromGalleryCompletionCall(path, onComplete);
            });
            return;

            async Task GetAudioClipFromGalleryCompletionCall(string path, Action<AudioUIData> completionCallback)
            {
                var audioUIData = await GetAudioClipFromPathAsync(path);
                completionCallback.Invoke(audioUIData);
            }
        }

        public async Task<AudioUIData> GetAudioClipFromDataBaseAsync(string audioName)
        {
            var mediaAsset = _sqlController.GetMediaAsset((int)MediaAssetType.Audio, audioName);
            var audioUIData = await GetAudioClipFromPathAsync(mediaAsset);
            return audioUIData;
        }

        public async Task<AudioUIData> GetAudioClipFromDataBasePathAsync(string path)
        {
            var mediaAsset = _sqlController.GetMediaAsset(path);
            var audioUIData = await GetAudioClipFromPathAsync(mediaAsset);
            return audioUIData;
        }

        async Task<AudioUIData> GetAudioClipFromPathAsync(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var mediaAssetData = new MediaAssetData
            {
                Name = fileName,
                Path = path,
                Type = MediaAssetType.Audio,
            };

            var audioUIData = await GetAudioClipFromPathAsync(mediaAssetData);
            return audioUIData;
        }

        async Task<AudioUIData> GetAudioClipFromPathAsync(MediaAssetData mediaAssetData)
        {
            var cachedAudioClip = GetAudioFromCache(mediaAssetData.Path);
            if (cachedAudioClip != null)
            {
                return cachedAudioClip;
            }

            var audioClip = await NativeBrowserController.GetAudioClipFromPathAsync(mediaAssetData.Path);
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

            NativeBrowserController.SaveAudioToGallery(audioUIData.Path, (fullPath, fileName) =>
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    onComplete?.Invoke(audioUIData);
                    return;
                }

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

        public bool IsAudioInDatabase(string path)
        {
            return _sqlController.ExistsMediaAsset(path);
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

        public async Task CreateCharacterAsync(CharacterUIData characterUIData)
        {
            await CreateCharacterAssetsAsync(characterUIData);

            var characterData = characterUIData.ToCharacterData();
            _sqlController.AddCharacter(characterData);
        }

        public async Task UpdateCharacterAsync(CharacterUIData characterUIData)
        {
            await CreateCharacterAssetsAsync(characterUIData);

            var characterData = characterUIData.ToCharacterData();
            _sqlController.UpdateCharacter(characterData);
        }

        async Task CreateCharacterAssetsAsync(CharacterUIData characterUIData)
        {
            var textureCompletion = new TaskCompletionSource<bool>();
            CreateTexture(characterUIData.Avatar, _ =>
            {
                textureCompletion.SetResult(true);
            });
            await textureCompletion.Task;
            foreach (var audioUIData in characterUIData.AudioList)
            {
                var audioCompletion = new TaskCompletionSource<bool>();
                CreateAudio(audioUIData, _ =>
                {
                    audioCompletion.SetResult(true);
                });
                await audioCompletion.Task;
            }
        }

        public bool IsCharacterInDatabase(CharacterUIData characterUIData)
        {
            return IsCharacterInDatabase(characterUIData.Name);
        }

        bool IsCharacterInDatabase(string characterName)
        {
            var characterData = _sqlController.GetCharacter(characterName);
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

        #region File Batch

        public async Task CreateCharactersFromBatch()
        {
            var path = await NativeBrowserController.PickFileAsync("txt");

            if (string.IsNullOrEmpty(path))
                return;

            await CreateCharactersFromFileAsync(path);

            Debug.Log("File batch created successfully!");
        }

        async Task CreateCharactersFromFileAsync(string path)
        {
            try
            {
                string fileContent = await NativeBrowserController.ReadFileAsync(path);
                var characterBatchDataList = JsonUtility.FromJson<CharacterBatchDataList>(fileContent);
                if (characterBatchDataList == null)
                    return;

                foreach (var characterBatchData in characterBatchDataList.CharacterList)
                {
                    var characterName = characterBatchData.Name;
                    if (IsCharacterInDatabase(characterName))
                        continue;

                    var defaultCharacter = await GetDefaultCharacterAsync();

                    // Name
                    defaultCharacter.Name = characterName;

                    // Avatar
                    if (!string.IsNullOrEmpty(characterBatchData.AvatarPath))
                    {
                        defaultCharacter.Avatar = IsTextureInDatabase(characterBatchData.AvatarPath)
                            ? await GetTextureFromDataBaseAsync(characterBatchData.AvatarPath)
                            : await GetTextureFromPathAsync(characterBatchData.AvatarPath, MediaAssetType.Avatar);
                    }

                    // Audio
                    var audioList = new List<AudioUIData>();
                    foreach (var audioPath in characterBatchData.AudioPaths)
                    {
                        var audioData = IsAudioInDatabase(audioPath)
                            ? await GetAudioClipFromDataBasePathAsync(audioPath)
                            : await GetAudioClipFromPathAsync(audioPath);

                        audioList.Add(audioData);
                    }
                    if (audioList.Any())
                    {
                        defaultCharacter.AudioList = audioList;
                    }

                    await CreateCharacterAsync(defaultCharacter);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        #endregion
    }
}