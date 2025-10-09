using System;
using System.Linq;
using SQLite;

namespace DnDInitiativeTracker.SQLData
{
    [Table("Character")]
    public record CharacterSQLData(
        int Id,
        bool Enabled,
        long InputDate,
        [property: Column("avatarMediaAssetId")] int AvatarMediaAssetId,
        [property: Column("name")] string Name,
        [property: Column("audioMediaAssetIds")] string AudioMediaAssetIds) : SQLiteData(Id, Enabled, InputDate)
    {
        int[] _audioAssetIdList = null;
        public int[] AudioAssetIdList => _audioAssetIdList ??= GetAudioAssetIds();

        public CharacterSQLData() : this(0, true, DateTime.Now.Ticks, 0, string.Empty, "0")
        {
        }

        int[] GetAudioAssetIds()
        {
            if (string.IsNullOrEmpty(AudioMediaAssetIds))
                return Array.Empty<int>();

            var splitIds = AudioMediaAssetIds.Split(',');
            return splitIds.Select(int.Parse).ToArray();
        }
    }
}