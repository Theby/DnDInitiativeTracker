using System.Collections.Generic;
using DnDInitiativeTracker.Extensions;
using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public class CurrentConfigurationData : FromSQLData<CurrentConfigurationSQLData>
    {
        public List<CharacterData> Characters { get; set; } = new();
        public List<int> InitiativeList { get; set; } = new();
        public MediaAssetData Background { get; set; }

        public CurrentConfigurationData() { }

        public CurrentConfigurationData(CurrentConfigurationSQLData sqlData, List<CharacterData> characters,
            List<int> initiativeList, MediaAssetData backgroundData) : base(sqlData) =>
            (Characters, InitiativeList, Background) = (characters, initiativeList, backgroundData);

        public override CurrentConfigurationSQLData ToSQLData() =>
            new(
                SQLId,
                Enabled,
                InputDate,
                Characters.ToIdList(x => x.SQLId),
                InitiativeList.ToIdList(x => x),
                Background?.SQLId ?? 0
            );
    }
}