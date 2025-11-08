using System.Collections.Generic;
using DnDInitiativeTracker.GameData;

namespace DnDInitiativeTracker.UIData
{
    public class CharacterUIData
    {
        public TextureUIData Avatar { get; set; }
        public string Name { get; set; }
        public List<AudioUIData> AudioList { get; set; }

        readonly CharacterData _characterData;

        public CharacterUIData(CharacterData characterData, TextureUIData avatarData, string name,
            List<AudioUIData> audioList)
        {
            _characterData = characterData;
            Avatar = avatarData;
            Name = name;
            AudioList = audioList;
        }

        public CharacterData ToCharacterData()
        {
            var avatarData = Avatar.ToMediaAssetData();
            var characterName = Name;
            var audioDataList = new List<MediaAssetData>();
            foreach (var audioUIData in AudioList)
            {
                if (audioUIData.Data == null)
                    continue;

                var audioData = audioUIData.ToMediaAssetData();
                audioDataList.Add(audioData);
            }

            return new CharacterData
            {
                SQLId = _characterData.SQLId,
                Enabled = _characterData.Enabled,
                InputDate = _characterData.InputDate,
                Avatar = avatarData,
                Name = characterName,
                AudioList = audioDataList,
            };
        }
    }
}