using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.Controller;
using DnDInitiativeTracker.GameData;
using DnDInitiativeTracker.SQLData;
using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class DnDITManager : MonoBehaviour
    {
        SQLController _sqlController;

        public void Initialize()
        {
            _sqlController = new SQLController();
            _sqlController.Initialize("DnDIT");

            CreateSQLTables();
        }

        void OnDestroy()
        {
            _sqlController.Clear();
        }

        void CreateSQLTables()
        {
            _sqlController.CreateTable<MediaAssetSQLData>();
            _sqlController.CreateTable<CharacterSQLData>();
            _sqlController.CreateTable<BackgroundSQLData>();
            _sqlController.CreateTable<CurrentConfigurationSQLData>();
        }

        public void AddMediaAsset(MediaAssetData mediaAsset)
        {
            var mediaAssetSQL = new MediaAssetSQLData
            {
                Name = mediaAsset.Name,
                Type = mediaAsset.Type,
                Path = mediaAsset.Path,
            };
            _sqlController.Insert(mediaAssetSQL);
        }

        public MediaAssetData GetMediaAssetById(int id)
        {
            var sqlData = _sqlController.GetById<MediaAssetSQLData>(id);
            if (sqlData == null)
                return null;

            var mediaAsset = CreateMediaAssetData(sqlData);
            return mediaAsset;
        }

        public MediaAssetData GetMediaAssetByType(string type)
        {
            var sqlData = _sqlController.GetBy<MediaAssetSQLData>(x => x.Type == type);
            if (sqlData == null)
                return null;

            var mediaAsset = CreateMediaAssetData(sqlData);
            return mediaAsset;
        }

        public Dictionary<string, MediaAssetData> GetMediaAssetsByType(string type)
        {
            var data = _sqlController.GetAllBy<MediaAssetSQLData>(m => m.Type == type);
            return data.Select(CreateMediaAssetData).ToDictionary(m => m.Name);
        }

        MediaAssetData CreateMediaAssetData(MediaAssetSQLData sqlData)
        {
            var mediaAsset = new MediaAssetData(sqlData);
            return mediaAsset;
        }

        public void AddCharacter(CharacterData character)
        {
            var characterSQL = new CharacterSQLData
            {
                Name = character.Name,
                AvatarMediaAssetId = character.AvatarData.SQLId,
                AudioMediaAssetIds = string.Join(",", character.AudioDataList.Select(a => a.SQLId))
            };
            _sqlController.Insert(characterSQL);
        }

        public CharacterData GetCharacterById(int id)
        {
            var sqlData = _sqlController.GetById<CharacterSQLData>(id);
            if (sqlData == null)
                return null;

            var character = CreateCharacterData(sqlData);
            return character;
        }

        public void UpdateCharacter(CharacterData character)
        {
            var sqlData = character.ToSQLData();
            _sqlController.Update(sqlData);
        }

        public Dictionary<string, CharacterData> GetAllCharacters()
        {
            var data = _sqlController.GetAll<CharacterSQLData>();
            return data.Select(CreateCharacterData).ToDictionary(character => character.Name);
        }

        CharacterData CreateCharacterData(CharacterSQLData sqlData)
        {
            var avatarData = GetMediaAssetById(sqlData.AvatarMediaAssetId);
            var audioDataList = sqlData.AudioAssetIdList.Select(GetMediaAssetById).ToList();
            var character = new CharacterData(sqlData, avatarData, audioDataList);

            return character;
        }

        public void AddBackground(BackgroundData background)
        {
            var backgroundSQL = new BackgroundSQLData
            {
                MediaAssetId = background.MediaAssetData.SQLId
            };
            _sqlController.Insert(backgroundSQL);
        }

        public BackgroundData GetBackgroundById(int id)
        {
            var sqlData = _sqlController.GetById<BackgroundSQLData>(id);
            if (sqlData == null)
                return null;

            var background = CreateBackgroundData(sqlData);

            return background;
        }

        public Dictionary<string, BackgroundData> GetAllBackgrounds()
        {
            var data = _sqlController.GetAll<BackgroundSQLData>();
            return data.Select(CreateBackgroundData).ToDictionary(bg => bg.MediaAssetData.Name);
        }

        BackgroundData CreateBackgroundData(BackgroundSQLData sqlData)
        {
            var mediaAssetData = GetMediaAssetById(sqlData.MediaAssetId);
            var background = new BackgroundData(sqlData, mediaAssetData);

            return background;
        }

        public void AddCurrentConfiguration(CurrentConfigurationData currentConfiguration)
        {
            var configSQL = new CurrentConfigurationSQLData
            {
                BackgroundId = currentConfiguration.Background.SQLId
            };
            _sqlController.Insert(configSQL);
        }

        public CurrentConfigurationData GetCurrentConfigurationById(int id)
        {
            var sqlData = _sqlController.GetById<CurrentConfigurationSQLData>(id);
            if (sqlData == null)
                return null;

            var backgroundData = GetBackgroundById(sqlData.BackgroundId);
            var config = new CurrentConfigurationData(sqlData, backgroundData);

            return config;
        }

        public void UpdateCurrentConfiguration(CurrentConfigurationData currentConfiguration)
        {
            var sqlData = currentConfiguration.ToSQLData();
            _sqlController.Update(sqlData);
        }
    }
}