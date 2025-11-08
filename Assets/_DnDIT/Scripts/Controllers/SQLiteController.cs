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

        public void Dispose()
        {
            _sqLiteService?.Clear();
        }

        void CreateSQLTables()
        {
            _sqLiteService.CreateTable<MediaAssetSQLData>();
            _sqLiteService.CreateTable<CharacterSQLData>();
            _sqLiteService.CreateTable<CurrentConfigurationSQLData>();
        }

        #region Media Assets

        public bool IsMediaAssetsTableEmpty()
        {
            return _sqLiteService.IsTableEmpty<MediaAssetSQLData>();
        }

        public MediaAssetData AddMediaAsset(MediaAssetData mediaAsset)
        {
            var mediaAssetSQL = mediaAsset.ToSQLData();
            _sqLiteService.Insert(mediaAssetSQL);

            mediaAsset.UpdateRegister(mediaAssetSQL);

            return mediaAsset;
        }

        public MediaAssetData GetMediaAsset(int id)
        {
            var sqlData = _sqLiteService.GetById<MediaAssetSQLData>(id);
            if (sqlData == null)
                return null;

            var mediaAsset = CreateMediaAssetData(sqlData);
            return mediaAsset;
        }

        public List<MediaAssetData> GetMediaAssets(int type)
        {
            var data = _sqLiteService.GetAllBy<MediaAssetSQLData>(m => m.Type == type);
            return data.Select(CreateMediaAssetData).ToList();
        }

        public MediaAssetData GetMediaAsset(int type, string name)
        {
            var sqlData = _sqLiteService.GetTableQuery<MediaAssetSQLData>()
                .Where(m => m.Type == type)
                .FirstOrDefault(m => m.Name == name);
            if (sqlData == null)
                return null;

            var mediaAsset = CreateMediaAssetData(sqlData);
            return mediaAsset;
        }

        MediaAssetData CreateMediaAssetData(MediaAssetSQLData sqlData)
        {
            var name = sqlData.Name;
            var type = (MediaAssetType)sqlData.Type;
            var path = sqlData.Path;

            var mediaAsset = new MediaAssetData(sqlData, name, type, path);
            return mediaAsset;
        }

        public List<string> GetAllMediaTypeNames(MediaAssetType type)
        {
            var names =_sqLiteService.GetTableQuery<MediaAssetSQLData>()
                .Where(m => m.Type == (int)type)
                .Select(m => m.Name);
            return names.ToList();
        }

        public bool ExistsMediaAsset(int id)
        {
            var sqlData = _sqLiteService.GetById<MediaAssetSQLData>(id);
            return sqlData != null;
        }

        #endregion

        #region Character

        public bool IsCharacterTableEmpty()
        {
            return _sqLiteService.IsTableEmpty<CharacterSQLData>();
        }

        public CharacterData AddCharacter(CharacterData character)
        {
            var sqlData = CreateCharacterSQLData(character);
            _sqLiteService.Insert(sqlData);

            character.UpdateRegister(sqlData);

            return character;
        }

        public void UpdateCharacter(CharacterData character)
        {
            var sqlData = CreateCharacterSQLData(character);
            _sqLiteService.Update(sqlData);

            character.UpdateRegister(sqlData);
        }

        public CharacterData GetCharacter(int id)
        {
            var sqlData = _sqLiteService.GetById<CharacterSQLData>(id);
            if (sqlData == null)
                return null;

            var character = CreateCharacterData(sqlData);
            return character;
        }

        public CharacterData GetCharacter(string name)
        {
            var sqlData = _sqLiteService.GetBy<CharacterSQLData>(c => c.Name == name);
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

        public List<string> GetAllCharactersNames()
        {
            var names =_sqLiteService.GetTableQuery<CharacterSQLData>().Select(c => c.Name);
            return names.ToList();
        }

        CharacterData CreateCharacterData(CharacterSQLData sqlData)
        {
            var avatarData = GetMediaAsset(sqlData.AvatarId);
            var name = sqlData.Name;
            var audioDataList = sqlData.AudioIdList.Select(GetMediaAsset).ToList();
            var character = new CharacterData(sqlData, avatarData, name, audioDataList);

            return character;
        }

        CharacterSQLData CreateCharacterSQLData(CharacterData character)
        {
            var avatarSQLData = character.Avatar.ToSQLData();
            if (!ExistsMediaAsset(avatarSQLData.Id))
            {
                _sqLiteService.Insert(avatarSQLData);
                character.Avatar.UpdateRegister(avatarSQLData);
            }

            foreach (var audioData in character.AudioList)
            {
                var audioSQLData = audioData.ToSQLData();
                if (ExistsMediaAsset(audioSQLData.Id))
                    continue;

                _sqLiteService.Insert(audioSQLData);
                audioData.UpdateRegister(audioSQLData);
            }

            var sqlData = character.ToSQLData();
            return sqlData;
        }

        public bool ExistsCharacter(int id)
        {
            var sqlData = _sqLiteService.GetById<CharacterSQLData>(id);
            return sqlData != null;
        }

        #endregion

        #region Current Configuration

        public bool IsCurrentConfigurationTableEmpty()
        {
            return _sqLiteService.IsTableEmpty<CurrentConfigurationSQLData>();
        }

        public CurrentConfigurationData AddCurrentConfiguration(CurrentConfigurationData currentConfiguration)
        {
            var configSQL = CreateCurrentConfigurationSQLData(currentConfiguration);
            _sqLiteService.Insert(configSQL);

            currentConfiguration.UpdateRegister(configSQL);

            return currentConfiguration;
        }

        public void UpdateCurrentConfiguration(CurrentConfigurationData currentConfiguration)
        {
            var configSQL = CreateCurrentConfigurationSQLData(currentConfiguration);
            _sqLiteService.Update(configSQL);

            currentConfiguration.UpdateRegister(configSQL);
        }

        public CurrentConfigurationData GetCurrentConfigurationById(int id)
        {
            var sqlData = _sqLiteService.GetById<CurrentConfigurationSQLData>(id);
            if (sqlData == null)
                return null;

            var characterDataList = sqlData.CharacterIdList.Select(GetCharacter).ToList();
            var initiativeList = sqlData.InitiativeList.ToList();
            var backgroundData = GetMediaAsset(sqlData.BackgroundId);
            var config = new CurrentConfigurationData(sqlData, characterDataList, initiativeList, backgroundData);

            return config;
        }

        CurrentConfigurationSQLData CreateCurrentConfigurationSQLData(CurrentConfigurationData configData)
        {
            foreach (var character in configData.Characters)
            {
                UpdateCharacter(character);
            }

            var sqlData = configData.ToSQLData();
            return sqlData;
        }

        #endregion
    }
}