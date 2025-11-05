using System.Collections.Generic;
using DnDInitiativeTracker.GameData;

namespace DnDInitiativeTracker.UIData
{
    public class CharacterUIData
    {
        public TextureUIData Avatar { get; set; }
        public string Name { get; set; }
        public List<AudioUIData> AudioClips { get; set; }

        public void SetData(CharacterUIData characterUIData)
        {
            Avatar = characterUIData.Avatar;
            Name = characterUIData.Name;
            AudioClips = characterUIData.AudioClips;
        }

        public CharacterData ToCharacterData()
        {
            var avatarData = Avatar.ToMediaAssetData();
            var characterName = Name;
            var audioDataList = new List<MediaAssetData>();
            foreach (var audioUIData in AudioClips)
            {
                if (audioUIData.Data == null)
                    continue;

                var audioData = audioUIData.ToMediaAssetData();
                audioDataList.Add(audioData);
            }

            return new CharacterData
            {
                Avatar = avatarData,
                Name = characterName,
                AudioList = audioDataList,
            };
        }
    }
}