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
        const int DefaultBackgroundID = 1;

        public CurrentConfigurationData CurrentConfiguration { get; private set; }

        public List<CharacterUIData> CurrentEncounter { get; set; }
        public BackgroundUIData CurrentBackground { get; set; }

        public bool IsLoaded { get; private set; }

        public event Action OnDataUpdated;

        SQLiteController _sqlController;

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
            AddDefaultBackground();
        }

        void AddDefaultMediaAssets()
        {
            if (!_sqlController.IsMediaAssetsTableEmpty())
                return;

            var defaultAvatarData = new MediaAssetData
            {
                Name = "DefaultAvatar",
                Type = NativeGallery.MediaType.Image,
                Path = Path.Combine(Application.streamingAssetsPath, "Textures/DefaultAvatar.jpg")
            };
            var defaultAudio1Data = new MediaAssetData
            {
                Name = "DefaultAudio1",
                Type = NativeGallery.MediaType.Audio,
                Path = Path.Combine(Application.streamingAssetsPath, "Audios/DefaultAudio1.mp3")
            };
            var defaultAudio2Data = new MediaAssetData
            {
                Name = "DefaultAudio2",
                Type = NativeGallery.MediaType.Audio,
                Path = Path.Combine(Application.streamingAssetsPath, "Audios/DefaultAudio2.mp3")
            };
            var defaultAudio3Data = new MediaAssetData
            {
                Name = "DefaultAudio3",
                Type = NativeGallery.MediaType.Audio,
                Path = Path.Combine(Application.streamingAssetsPath, "Audios/DefaultAudio3.mp3")
            };

            _sqlController.AddMediaAsset(defaultAvatarData);
            _sqlController.AddMediaAsset(defaultAudio1Data);
            _sqlController.AddMediaAsset(defaultAudio2Data);
            _sqlController.AddMediaAsset(defaultAudio3Data);
        }

        void AddDefaultBackground()
        {
            if (!_sqlController.IsBackgroundTableEmpty())
                return;

            var defaultBackgroundData = new BackgroundData
            {
                MediaAssetData = new MediaAssetData
                {
                    Name = "DefaultBG",
                    Type = NativeGallery.MediaType.Image,
                    Path = Path.Combine(Application.streamingAssetsPath, "Textures/DefaultBG.jpg")
                }
            };

            _sqlController.AddBackground(defaultBackgroundData);
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
            CurrentConfigurationData config;
            if (_sqlController.IsCurrentConfigurationTableEmpty())
            {
                config = new CurrentConfigurationData
                {
                    Background = _sqlController.GetBackgroundById(DefaultBackgroundID)
                };
                _sqlController.AddCurrentConfiguration(config);
            }
            else
            {
                config = _sqlController.GetCurrentConfigurationById(CurrentConfigID);
            }

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
                var audioClips = new List<AudioClip>();
                foreach (var audioData in character.AudioDataList)
                {
                    yield return NativeGalleryController.GetAudioClipFromPathAsync(audioData.Path, clip =>
                    {
                        audioClips.Add(clip);
                    });
                }

                var characterUIData = new CharacterUIData
                {
                    AvatarTexture = NativeGalleryController.GetImageFromPath(character.AvatarData.Path),
                    Name = character.Name,
                    AudioClips = audioClips,
                    Initiative = character.Initiative
                };
                currentEncounter.Add(characterUIData);
            }
            CurrentEncounter = currentEncounter;
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
                BackgroundTexture = NativeGalleryController.GetImageFromPath(CurrentConfiguration.Background.MediaAssetData.Path)
            };
            CurrentBackground = backgroundUIData;
        }

        #endregion

        public List<string> GetAllCharacterNames()
        {
            return _sqlController.GetAllCharactersNames();
        }

        public List<string> GetAllBackgroundNames()
        {
            return _sqlController.GetAllBackgroundNames();
        }

        public List<string> GetAllAudioNames()
        {
            return _sqlController.GetAllAudioNames();
        }

        public void GetAvatarFromGallery(Action<string, Texture> onComplete)
        {
            NativeGalleryController.GetImagePathFromGallery(path =>
            {
                var texture = NativeGalleryController.GetImageFromPath(path);
                onComplete?.Invoke(path, texture);
            });
        }

        public void GetAudioClipFromGallery(Action<string, AudioClip> onComplete)
        {
            NativeGalleryController.GetAudioPathFromGallery(path =>
            {
                NativeGalleryController.GetAudioClipFromPath(path, audioClip =>
                {
                    onComplete?.Invoke(path, audioClip);
                });
            });
        }

        public void GetAudioClipByName(string audioName, Action<string, AudioClip> onComplete)
        {
            var mediaAsset = _sqlController.GetMediaAssetByNameAndType(audioName, "Audio");
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
            List<AudioClip> audioClips = new();
            foreach (var audioData in characterData.AudioDataList)
            {
                var audioClip = await NativeGalleryController.GetAudioClipFromPathAsync(audioData.Path);
                audioClips.Add(audioClip);
            }

            var characterUIData = new CharacterUIData
            {
                AvatarTexture = NativeGalleryController.GetImageFromPath(characterData.AvatarData.Path),
                AvatarPath = characterData.AvatarData.Path,
                Name = characterData.Name,
                AudioClips = audioClips,
                AudioClipPaths = characterData.AudioDataList.ConvertAll(x => x.Path),
                Initiative = characterData.Initiative,
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
            var backgroundData = new BackgroundData
            {
                MediaAssetData = new MediaAssetData
                {
                    Name = fileName,
                    Type = NativeGallery.MediaType.Image,
                    Path = fullPath,
                },
            };
            backgroundData.SQLId = _sqlController.AddBackground(backgroundData);

            CurrentConfiguration.Background = backgroundData;
            _sqlController.UpdateCurrentConfiguration(CurrentConfiguration);
            LoadBackgroundUIData();

            OnDataUpdated?.Invoke();
        }

        public void UpdateCurrentBackground(string bgName)
        {
            var backgroundData = _sqlController.GetBackgroundByName(bgName);
            CurrentConfiguration.Background = backgroundData;
            _sqlController.UpdateCurrentConfiguration(CurrentConfiguration);
            LoadBackgroundUIData();

            OnDataUpdated?.Invoke();
        }
    }
}