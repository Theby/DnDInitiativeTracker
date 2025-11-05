using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.GameData;

namespace DnDInitiativeTracker.UIData
{
    public class CurrentConfigurationUIData
    {
        public List<CharacterUIData> CurrentEncounter { get; set; }
        public List<int> InitiativeList { get; set; }
        public TextureUIData CurrentBackground { get; set; }

        public CurrentConfigurationData ToCurrentConfigurationData()
        {
            var characters = CurrentEncounter.Select(c => c.ToCharacterData()).ToList();
            var initiativeList = InitiativeList;
            var backgroundData = CurrentBackground.ToMediaAssetData();

            return new CurrentConfigurationData
            {
                Characters = characters,
                InitiativeList = initiativeList,
                Background = backgroundData,
            };
        }
    }
}