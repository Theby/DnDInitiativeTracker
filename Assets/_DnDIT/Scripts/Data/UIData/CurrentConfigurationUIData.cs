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

        readonly CurrentConfigurationData _configurationData;

        public CurrentConfigurationUIData(CurrentConfigurationData configurationData,
            List<CharacterUIData> currentEncounter, List<int> initiativeList, TextureUIData currentBackground)
        {
            _configurationData = configurationData;
            CurrentEncounter = currentEncounter;
            InitiativeList = initiativeList;
            CurrentBackground = currentBackground;
        }

        public CurrentConfigurationData ToCurrentConfigurationData()
        {
            var characters = CurrentEncounter.Select(c => c.ToCharacterData()).ToList();
            var initiativeList = InitiativeList;
            var backgroundData = CurrentBackground.ToMediaAssetData();

            return new CurrentConfigurationData
            {
                SQLId = _configurationData.SQLId,
                Enabled = _configurationData.Enabled,
                InputDate = _configurationData.InputDate,
                Characters = characters,
                InitiativeList = initiativeList,
                Background = backgroundData,
            };
        }
    }
}