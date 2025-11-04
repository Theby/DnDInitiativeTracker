using System.Collections.Generic;
using DnDInitiativeTracker.GameData;

namespace DnDInitiativeTracker.UIData
{
    public class CharacterUIData
    {
        public AvatarUIData Avatar { get; set; }
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
            var avatarData = new MediaAssetData
            {
                Name = Avatar.Name,
                Type = MediaAssetType.Avatar,
                Path = Avatar.FilePath,
            };
            var characterName = Name;
            var audioDataList = new List<MediaAssetData>();
            foreach (var audioClip in AudioClips)
            {
                if (audioClip.AudioClip == null)
                    continue;

                var audioData = new MediaAssetData
                {
                    Name = audioClip.Name,
                    Type = MediaAssetType.Audio,
                    Path = audioClip.FilePath,
                };
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