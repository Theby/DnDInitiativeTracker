using System.Collections.Generic;

namespace DnDInitiativeTracker.UIData
{
    public class DMScreenData
    {
        public List<CharacterUIData> CurrentEncounter { get; set; } = new();
        public List<int> InitiativeList { get; set; } = new();
        public TextureUIData CurrentBackground { get; set; }

        public List<string> CharacterNames { get; set; } = new();
        public List<string> BackgroundNames { get; set; } = new();
        public List<string> AudioNames { get; set; } = new();
    }
}
