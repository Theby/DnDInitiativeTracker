using System;
using DnDInitiativeTracker.Extensions;
using SQLite;

namespace DnDInitiativeTracker.SQLData
{
    [Table("CurrentConfiguration")]
    public record CurrentConfigurationSQLData(
        int Id,
        bool Enabled,
        long InputDate,
        [property: Column("characterIds")] string CharacterIds,
        [property: Column("initiativeOrder")] string InitiativeOrder,
        [property: Column("backgroundId")] int BackgroundId) : SQLiteData(Id, Enabled, InputDate)
    {

        int[] _characterIdList = null;
        public int[] CharacterIdList => _characterIdList ??= CharacterIds.ToIntegerArray();

        int[] _initiativeList = null;
        public int[] InitiativeList => _initiativeList ??= InitiativeOrder.ToIntegerArray();

        public CurrentConfigurationSQLData() : this(0, true, DateTime.Now.Ticks, "0", "0", 0)
        {
        }
    }
}