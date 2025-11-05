using System.Collections.Generic;
using DnDInitiativeTracker.Extensions;
using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public class CharacterData : FromSQLData<CharacterSQLData>
    {
        public MediaAssetData Avatar { get; set; }
        public string Name { get; set; }
        public List<MediaAssetData> AudioList { get; set; }

        public CharacterData() { }

        public CharacterData(CharacterSQLData sqlData, MediaAssetData avatarData, string name,
            List<MediaAssetData> audioDataList) : base(sqlData) =>
            (Avatar, AudioList, Name) = (avatarData, audioDataList, name);

        public override CharacterSQLData ToSQLData() =>
            new(
                SQLId,
                Enabled,
                InputDate,
                Avatar?.SQLId ?? 0,
                Name,
                AudioList.ToIdList(x => x.SQLId)
            );
    }
}