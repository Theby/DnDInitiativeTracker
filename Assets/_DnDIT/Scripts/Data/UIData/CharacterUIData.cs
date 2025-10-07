using System.Collections.Generic;
using UnityEngine;

namespace DnDInitiativeTracker.UIData
{
    public class CharacterUIData
    {
        public Texture2D AvatarTexture { get; set; }
        public string Name { get; set; }
        public List<AudioClip> AudioClips { get; set; }
        public int Initiative { get; set; }
    }
}