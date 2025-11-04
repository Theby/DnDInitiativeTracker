using System;
using DnDInitiativeTracker.Extensions;
using SQLite;

namespace DnDInitiativeTracker.SQLData
{
    [Table("Character")]
    public record CharacterSQLData(
        int Id,
        bool Enabled,
        long InputDate,
        [property: Column("avatarId")] int AvatarId,
        [property: Column("name")] string Name,
        [property: Column("audioIds")] string AudioIds) : SQLiteData(Id, Enabled, InputDate)
    {
        int[] _audioIdList = null;
        public int[] AudioIdList => _audioIdList ??= AudioIds.ToIntegerArray();

        public CharacterSQLData() : this(0, true, DateTime.Now.Ticks, 0, string.Empty, "0")
        {
        }
    }
}