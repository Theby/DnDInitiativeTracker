using System.Collections.Generic;

namespace DnDInitiativeTracker.UIData
{
    public class DMScreenData
    {
        public List<CharacterUIData> CurrentEncounter { get; set; } = new();
        public List<string> CharacterNames { get; set; } = new();

        public BackgroundUIData CurrentBackground { get; set; }
        public List<string> BackgroundNames { get; set; } = new();
    }
}
