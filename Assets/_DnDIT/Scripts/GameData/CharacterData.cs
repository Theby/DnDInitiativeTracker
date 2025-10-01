using System;
using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public class CharacterData : FromSQLData<CharacterSQLData>
    {
        public MediaAssetData AvatarData { get; set; }
        public string Name { get; set; }
        public List<MediaAssetData> AudioDataList { get; set; }

        public CharacterData(CharacterSQLData sqlData, MediaAssetData avatarData, List<MediaAssetData> audioDataList)
            : base(sqlData)
        {
            AvatarData = avatarData;
            AudioDataList = audioDataList;

            if (sqlData == null)
                return;

            Name = sqlData.Name;
        }

        public override CharacterSQLData ToSQLData()
        {
            var audioIdList = string.Join(",", AudioDataList.Select(x => x.SQLId));
            return new CharacterSQLData(
                SQLId,
                Enabled,
                InputDate,
                AvatarData?.SQLId ?? -1,
                Name,
                audioIdList
            );
        }
    }
}