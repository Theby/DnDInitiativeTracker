using System.Collections.Generic;

namespace DnDInitiativeTracker.UIData
{
    public class DMScreenData
    {
        public CurrentConfigurationUIData CurrentConfigurationUIData { get; set; }

        public List<string> CharacterNames { get; set; } = new();
        public List<string> BackgroundNames { get; set; } = new();
        public List<string> AudioNames { get; set; } = new();
    }
}
