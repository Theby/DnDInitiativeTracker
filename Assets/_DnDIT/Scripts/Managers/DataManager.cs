using System.Collections.Generic;
using DnDInitiativeTracker.Controller;
using DnDInitiativeTracker.GameData;
using UnityEngine;
using UnityEngine.UIElements;

namespace DnDInitiativeTracker.Manager
{
    public class DataManager : MonoBehaviour
    {
        const int CurrentConfigID = 1;
        public CurrentConfigurationData CurrentConfiguration { get; private set; }
        public CharacterData DefaultCharacter { get; private set; }
        public BackgroundData DefaultBackground { get; private set; }

        SQLiteController _sqlController;

        public void Initialize()
        {
            _sqlController.Initialize();
            LoadData();
        }

        void OnDestroy()
        {
        }

        void LoadData()
        {
            CurrentConfiguration = _sqlController.GetCurrentConfigurationById(CurrentConfigID);
            DefaultCharacter = new CharacterData
            {
                AvatarData = new MediaAssetData
                {
                    Name = "DefaultAvatar",
                    Type = NativeGallery.MediaType.Image,
                    Path = "Textures/DefaultAvatar"
                },
                Name = "DefaultCharacter",
                AudioDataList = new List<MediaAssetData>
                {
                    new()
                    {
                        Name = "DefaultAudio1",
                        Type = NativeGallery.MediaType.Audio,
                        Path = "Audios/DefaultAudio1"
                    },
                    new()
                    {
                        Name = "DefaultAudio2",
                        Type = NativeGallery.MediaType.Audio,
                        Path = "Audios/DefaultAudio2"
                    },
                    new()
                    {
                        Name = "DefaultAudio3",
                        Type = NativeGallery.MediaType.Audio,
                        Path = "Audios/DefaultAudio3"
                    }
                },
                Initiative = 0
            };
            DefaultBackground = new BackgroundData
            {
                MediaAssetData = new MediaAssetData
                {
                    Name = "DefaultBG",
                    Type = NativeGallery.MediaType.Image,
                    Path = "Textures/DefaultBG"
                }
            };
        }

        public Dictionary<string, CharacterData> GetAllCharacters()
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, BackgroundData> GetAllBackgrounds()
        {
            throw new System.NotImplementedException();
        }

        public void AddBackground(BackgroundData backgroundData)
        {
            throw new System.NotImplementedException();
        }
    }
}