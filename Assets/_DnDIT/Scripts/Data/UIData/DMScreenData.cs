using System.Collections.Generic;

namespace DnDInitiativeTracker.UIData
{
    public class DMScreenData
    {
        public List<CharacterUIData> CurrentEncounter { get; set; } = new();
        public List<string> AllCharacterNames { get; set; } = new();

        public BackgroundUIData CurrentBackground { get; set; }
        public List<string> AllBackgroundNames { get; set; } = new();

        public CharacterUIData DefaultCharacter { get; set; }
        public BackgroundUIData DefaultBackground { get; set; }

    }
}
