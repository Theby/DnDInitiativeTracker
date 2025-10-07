using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.GameData;
using DnDInitiativeTracker.Service;
using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.Controller
{
    public class SQLiteController
    {
        const string DataBaseName = "DndIT";

        SQLiteService _sqLiteService;

        public void Initialize()
        {
            _sqLiteService = new SQLiteService();
            _sqLiteService.Initialize(DataBaseName);

            CreateSQLTables();
        }

        void Dispose()
        {
            _sqLiteService?.Clear();
        }

        void CreateSQLTables()
        {
            _sqLiteService.CreateTable<MediaAssetSQLData>();
            _sqLiteService.CreateTable<CharacterSQLData>();
            _sqLiteService.CreateTable<BackgroundSQLData>();
            _sqLiteService.CreateTable<CurrentConfigurationSQLData>();
        }

        #region Media Assets

        public void AddMediaAsset(MediaAssetData mediaAsset)
        {
            var mediaAssetSQL = mediaAsset.ToSQLData();
            _sqLiteService.Insert(mediaAssetSQL);
        }

        public MediaAssetData GetMediaAssetById(int id)
        {
            var sqlData = _sqLiteService.GetById<MediaAssetSQLData>(id);
            if (sqlData == null)
                return null;

            var mediaAsset = CreateMediaAssetData(sqlData);
            return mediaAsset;
        }

        public MediaAssetData GetMediaAssetByType(string type)
        {
            var sqlData = _sqLiteService.GetBy<MediaAssetSQLData>(x => x.Type == type);
            if (sqlData == null)
                return null;

            var mediaAsset = CreateMediaAssetData(sqlData);
            return mediaAsset;
        }

        public Dictionary<string, MediaAssetData> GetMediaAssetsByType(string type)
        {
            var data = _sqLiteService.GetAllBy<MediaAssetSQLData>(m => m.Type == type);
            return data.Select(CreateMediaAssetData).ToDictionary(m => m.Name);
        }

        MediaAssetData CreateMediaAssetData(MediaAssetSQLData sqlData)
        {
            var mediaAsset = new MediaAssetData(sqlData);
            return mediaAsset;
        }

        #endregion

        #region Character

        public void AddCharacter(CharacterData character)
        {
            var characterSQL = character.ToSQLData();
            _sqLiteService.Insert(characterSQL);
        }

        public CharacterData GetCharacterById(int id)
        {
            var sqlData = _sqLiteService.GetById<CharacterSQLData>(id);
            if (sqlData == null)
                return null;

            var character = CreateCharacterData(sqlData);
            return character;
        }

        public Dictionary<string, CharacterData> GetAllCharacters()
        {
            var data = _sqLiteService.GetAll<CharacterSQLData>();
            return data.Select(CreateCharacterData).ToDictionary(character => character.Name);
        }

        public void UpdateCharacter(CharacterData character)
        {
            var sqlData = character.ToSQLData();
            _sqLiteService.Update(sqlData);
        }

        CharacterData CreateCharacterData(CharacterSQLData sqlData)
        {
            var avatarData = GetMediaAssetById(sqlData.AvatarMediaAssetId);
            var audioDataList = sqlData.AudioAssetIdList.Select(GetMediaAssetById).ToList();
            var character = new CharacterData(sqlData, avatarData, audioDataList);

            return character;
        }

        #endregion

        #region Background

        public void AddBackground(BackgroundData background)
        {
            var backgroundSQL = background.ToSQLData();
            _sqLiteService.Insert(backgroundSQL);
        }

        public BackgroundData GetBackgroundById(int id)
        {
            var sqlData = _sqLiteService.GetById<BackgroundSQLData>(id);
            if (sqlData == null)
                return null;

            var background = CreateBackgroundData(sqlData);
            return background;
        }

        public Dictionary<string, BackgroundData> GetAllBackgrounds()
        {
            var data = _sqLiteService.GetAll<BackgroundSQLData>();
            return data.Select(CreateBackgroundData).ToDictionary(bg => bg.MediaAssetData.Name);
        }

        BackgroundData CreateBackgroundData(BackgroundSQLData sqlData)
        {
            var mediaAssetData = GetMediaAssetById(sqlData.MediaAssetId);
            var background = new BackgroundData(sqlData, mediaAssetData);

            return background;
        }

        #endregion

        #region Current Configuration

        public void AddCurrentConfiguration(CurrentConfigurationData currentConfiguration)
        {
            var configSQL = currentConfiguration.ToSQLData();
            _sqLiteService.Insert(configSQL);
        }

        public CurrentConfigurationData GetCurrentConfigurationById(int id)
        {
            var sqlData = _sqLiteService.GetById<CurrentConfigurationSQLData>(id);
            if (sqlData == null)
                return null;

            var characterDataList = sqlData.CharacterIdList.Select(GetCharacterById).ToList();
            var backgroundData = GetBackgroundById(sqlData.BackgroundId);
            var config = new CurrentConfigurationData(sqlData, characterDataList, backgroundData);

            return config;
        }

        public void UpdateCurrentConfiguration(CurrentConfigurationData currentConfiguration)
        {
            var sqlData = currentConfiguration.ToSQLData();
            _sqLiteService.Update(sqlData);
        }

        #endregion
    }
}