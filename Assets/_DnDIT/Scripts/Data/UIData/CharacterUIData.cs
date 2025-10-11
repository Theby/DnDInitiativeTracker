using System.Collections.Generic;
using DnDInitiativeTracker.GameData;
using UnityEngine;

namespace DnDInitiativeTracker.UIData
{
    public class CharacterUIData
    {
        public Texture AvatarTexture { get; set; }
        public string AvatarPath { get; set; }
        public string Name { get; set; }
        public List<AudioClip> AudioClips { get; set; } = new(){null, null, null};
        public List<string> AudioClipPaths { get; set; } = new(){string.Empty, string.Empty, string.Empty};
        public int Initiative { get; set; }

        public void SetData(CharacterUIData characterUIData)
        {
            AvatarTexture = characterUIData.AvatarTexture;
            AvatarPath = characterUIData.AvatarPath;
            Name = characterUIData.Name;
            AudioClips = characterUIData.AudioClips;
            AudioClipPaths = characterUIData.AudioClipPaths;
            Initiative = characterUIData.Initiative;
        }

        public CharacterData ToCharacterData()
        {
            var audioDataList = new List<MediaAssetData>();
            for (var i = 0; i < AudioClips.Count; i++)
            {
                //TODO may need to check if audio clip exists before doing this
                var audioClip = AudioClips[i];
                var audioPath = AudioClipPaths[i];
                audioDataList.Add(new MediaAssetData
                {
                    Name = audioClip.name,
                    Type = NativeGallery.MediaType.Audio,
                    Path = audioPath,
                });
            }

            return new CharacterData
            {
                AvatarData = new MediaAssetData
                {
                    Name = AvatarTexture.name,
                    Type = NativeGallery.MediaType.Image,
                    Path = AvatarPath,
                },
                Name = Name,
                AudioDataList = audioDataList,
                Initiative = Initiative
            };
        }

        public void Dispose()
        {
            if (AvatarTexture != null)
            {
                Texture.Destroy(AvatarTexture);
            }
            AvatarTexture = null;

            Name = string.Empty;

            foreach (var audioClip in AudioClips)
            {
                if(audioClip != null)
                {
                    AudioClip.Destroy(audioClip);
                }
            }
            AudioClips.Clear();
            AudioClipPaths.Clear();

            Initiative = 0;
        }
    }
}