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
        [property: Column("imageAssetId")] int ImageAssetId,
        [property: Column("name")] string Name,
        [property: Column("audioAssetIds")] string AudioAssetIds) : SQLiteData(Id, Enabled, InputDate)
    {
        int[] _audioAssetIdList = null;
        public int[] AudioAssetIdList => _audioAssetIdList ??= GetAudioAssetIds();

        public CharacterSQLData() : this(0, true, DateTime.Now.Ticks, -1, string.Empty, "{-1}")
        {
        }

        int[] GetAudioAssetIds()
        {
            var splitIds = AudioAssetIds.Split(',');
            return splitIds.Select(int.Parse).ToArray();
        }
    }
}