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
            _sqLiteService.CreateTable<BackgroundSQLData>();
            _sqLiteService.CreateTable<CurrentConfigurationSQLData>();
        }

        #region Media Assets

        public bool IsMediaAssetsTableEmpty()
        {
            return _sqLiteService.IsTableEmpty<MediaAssetSQLData>();
        }

        public int AddMediaAsset(MediaAssetData mediaAsset)
        {
            var mediaAssetSQL = mediaAsset.ToSQLData();
            _sqLiteService.Insert(mediaAssetSQL);

            return mediaAssetSQL.Id;
        }

        public MediaAssetData GetMediaAssetById(int id)
        {
            var sqlData = _sqLiteService.GetById<MediaAssetSQLData>(id);
            if (sqlData == null)
                return null;

            var mediaAsset = CreateMediaAssetData(sqlData);
            return mediaAsset;
        }

        public MediaAssetData GetMediaAssetByNameAndType(string name, string type)
        {
            var sqlDataList = _sqLiteService.GetAllBy<MediaAssetSQLData>(m => m.Type == type);
            var sqlData = sqlDataList.FirstOrDefault(m => m.Name == name);
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

        public List<string> GetAllAudioNames()
        {
            var audioDataList = _sqLiteService.GetAllBy<MediaAssetSQLData>(m => m.Type == "Audio");
            return audioDataList.Select(a => a.Name).ToList();
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
            var avatarAsset = GetMediaAssetByNameAndType(character.AvatarData.Name, "Image");
            if (avatarAsset == null)
            {
                var avatarSQLData = character.AvatarData.ToSQLData();
                _sqLiteService.Insert(avatarSQLData);

                character.AvatarData.SQLId = avatarSQLData.Id;
            }
            else
            {
                character.AvatarData.SQLId = avatarAsset.SQLId;
            }

            foreach (var audioData in character.AudioDataList)
            {
                var audioAsset = GetMediaAssetByNameAndType(audioData.Name, "Audio");
                if (audioAsset == null)
                {
                    var audioSQLData = audioData.ToSQLData();
                    _sqLiteService.Insert(audioSQLData);

                    audioData.SQLId = audioSQLData.Id;
                }
                else
                {
                    audioData.SQLId = audioAsset.SQLId;
                }
            }

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

        public CharacterData GetCharacterByName(string name)
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

        public void UpdateCharacter(CharacterData character)
        {
            var avatarAsset = GetMediaAssetByNameAndType(character.AvatarData.Name, "Image");
            if (avatarAsset == null)
            {
                var avatarSQLData = character.AvatarData.ToSQLData();
                _sqLiteService.Insert(avatarSQLData);

                character.AvatarData.SQLId = avatarSQLData.Id;
            }
            else
            {
                character.AvatarData.SQLId = avatarAsset.SQLId;
            }

            foreach (var audioData in character.AudioDataList)
            {
                var audioAsset = GetMediaAssetByNameAndType(audioData.Name, "Audio");
                if (audioAsset == null)
                {
                    var audioSQLData = audioData.ToSQLData();
                    _sqLiteService.Insert(audioSQLData);

                    audioData.SQLId = audioSQLData.Id;
                }
                else
                {
                    audioData.SQLId = audioAsset.SQLId;
                }
            }

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

        public bool IsBackgroundTableEmpty()
        {
            return _sqLiteService.IsTableEmpty<BackgroundSQLData>();
        }

        public int AddBackground(BackgroundData background)
        {
            if (background.MediaAssetData.SQLId == 0)
            {
                var mediaId = AddMediaAsset(background.MediaAssetData);
                background.MediaAssetData.SQLId = mediaId;
            }

            var backgroundSQL = background.ToSQLData();
            _sqLiteService.Insert(backgroundSQL);

            return backgroundSQL.Id;
        }

        public BackgroundData GetBackgroundById(int id)
        {
            var sqlData = _sqLiteService.GetById<BackgroundSQLData>(id);
            if (sqlData == null)
                return null;

            var background = CreateBackgroundData(sqlData);
            return background;
        }

        public BackgroundData GetBackgroundByName(string name)
        {
            var mediaAsset = _sqLiteService.GetBy<MediaAssetSQLData>(m => m.Name == name);
            if (mediaAsset == null)
                return null;

            var sqlData = _sqLiteService.GetBy<BackgroundSQLData>(b => b.MediaAssetId == mediaAsset.Id);
            if (sqlData == null)
                return null;

            return CreateBackgroundData(sqlData);
        }

        public List<string> GetAllBackgroundNames()
        {
            var mediaIds = _sqLiteService.GetTableQuery<BackgroundSQLData>().Select(b => b.MediaAssetId);
            var names = _sqLiteService.GetAllBy<MediaAssetSQLData>(m => mediaIds.Contains(m.Id)).Select(m => m.Name);
            return names.ToList();
        }

        BackgroundData CreateBackgroundData(BackgroundSQLData sqlData)
        {
            var mediaAssetData = GetMediaAssetById(sqlData.MediaAssetId);
            var background = new BackgroundData(sqlData, mediaAssetData);

            return background;
        }

        #endregion

        #region Current Configuration

        public bool IsCurrentConfigurationTableEmpty()
        {
            return _sqLiteService.IsTableEmpty<CurrentConfigurationSQLData>();
        }

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