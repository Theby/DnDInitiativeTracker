using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public class CurrentConfigurationData : FromSQLData<CurrentConfigurationSQLData>
    {
        public List<CharacterData> Characters { get; set; } = new();
        public BackgroundData Background { get; set; }

        public CurrentConfigurationData() { }

        public CurrentConfigurationData(CurrentConfigurationSQLData sqlData, List<CharacterData> characters, BackgroundData backgroundData)
            : base(sqlData)
        {
            Characters = characters;
            Background = backgroundData;
        }

        public override CurrentConfigurationSQLData ToSQLData()
        {
            var characterIdList = string.Join(",", Characters.Select(x => x.SQLId));
            return new CurrentConfigurationSQLData(
                SQLId,
                Enabled,
                InputDate,
                characterIdList,
                Background?.SQLId ?? 0
            );
        }
    }
}