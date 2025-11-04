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
        public BackgroundUIData CurrentBackground { get; set; }

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
                var audioClips = new List<AudioUIData>();
                foreach (var audioData in character.AudioList)
                {
                    yield return NativeGalleryController.GetAudioClipFromPathAsync(audioData.Path, clip =>
                    {
                        var audioClip = new AudioUIData
                        {
                            Name = audioData.Name,
                            FilePath = audioData.Path,
                            AudioClip = clip,
                        };
                        audioClips.Add(audioClip);
                    });
                }

                var characterUIData = new CharacterUIData
                {
                    Avatar = new AvatarUIData
                    {
                        Name = character.Avatar.Name,
                        FilePath = character.Avatar.Path,
                        AvatarTexture = NativeGalleryController.GetImageFromPath(character.Avatar.Path)
                    },
                    Name = character.Name,
                    AudioClips = audioClips,
                };
                currentEncounter.Add(characterUIData);
            }

            CurrentEncounter = currentEncounter;
            InitiativeList = CurrentConfiguration.InitiativeList;
        }

        void LoadBackgroundUIData()
        {
            if (CurrentBackground != null)
            {
                Destroy(CurrentBackground.BackgroundTexture);
            }

            var backgroundUIData = new BackgroundUIData
            {
                Name = CurrentConfiguration.Background.Name,
                FilePath = CurrentConfiguration.Background.Path,
                BackgroundTexture = NativeGalleryController.GetImageFromPath(CurrentConfiguration.Background.Path)
            };
            CurrentBackground = backgroundUIData;
        }

        #endregion

        public List<string> GetAllAvatarNames()
        {
            return _sqlController.GetAllMediaTypeNames(MediaAssetType.Avatar);
        }

        public List<string> GetAllAudioNames()
        {
            return _sqlController.GetAllMediaTypeNames(MediaAssetType.Audio);
        }

        public List<string> GetAllBackgroundNames()
        {
            return _sqlController.GetAllMediaTypeNames(MediaAssetType.Background);
        }

        public List<string> GetAllCharacterNames()
        {
            return _sqlController.GetAllCharactersNames();
        }

        public void GetAvatarFromGallery(Action<string, Texture> onComplete)
        {
            NativeGalleryController.GetImagePathFromGallery(path =>
            {
                NativeGalleryController.SaveImageToGallery(path, (fullPath, fileName) =>
                {
                    var texture = NativeGalleryController.GetImageFromPath(fullPath);
                    texture.name = fileName;
                    onComplete?.Invoke(fullPath, texture);
                });
            });
        }

        public void GetAudioClipFromGallery(Action<string, AudioClip> onComplete)
        {
            NativeGalleryController.GetAudioPathFromGallery(path =>
            {
                NativeGalleryController.SaveAudioToGallery(path, (fullPath, fileName) =>
                {
                    NativeGalleryController.GetAudioClipFromPath(fullPath, audioClip =>
                    {
                        onComplete?.Invoke(fullPath, audioClip);
                    });
                });
            });
        }

        public void GetAudioClipByName(string audioName, Action<string, AudioClip> onComplete)
        {
            var mediaAsset = _sqlController.GetMediaAsset((int)MediaAssetType.Audio, audioName);
            NativeGalleryController.GetAudioClipFromPath(mediaAsset.Path, audioClip => onComplete?.Invoke(mediaAsset.Path, audioClip));
        }

        public CharacterData GetCharacterByName(string characterName)
        {
            return _sqlController.GetCharacterByName(characterName);
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

        public void CreateCharacterUIData(CharacterData characterData, Action<CharacterUIData> onComplete)
        {
            CreateCharacterUIDataAsync(characterData, onComplete);
        }

        public async Task CreateCharacterUIDataAsync(CharacterData characterData, Action<CharacterUIData> onComplete)
        {
            var avatarUIData = new AvatarUIData
            {
                Name = characterData.Avatar.Name,
                FilePath = characterData.Avatar.Path,
                AvatarTexture = NativeGalleryController.GetImageFromPath(characterData.Avatar.Path),
            };

            var audioClips = new List<AudioUIData>();
            foreach (var audioData in characterData.AudioList)
            {
                var audioClip = await NativeGalleryController.GetAudioClipFromPathAsync(audioData.Path);

                var audioUIData = new AudioUIData
                {
                    Name = audioData.Name,
                    FilePath = audioData.Path,
                    AudioClip = audioClip,
                };
                audioClips.Add(audioUIData);
            }

            var characterUIData = new CharacterUIData
            {
                Avatar = avatarUIData,
                Name = characterData.Name,
                AudioClips = audioClips,
            };

            onComplete?.Invoke(characterUIData);
        }

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