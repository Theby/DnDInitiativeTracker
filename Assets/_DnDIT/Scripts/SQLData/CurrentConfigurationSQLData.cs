using System;
using SQLite;

namespace DnDInitiativeTracker.SQLData
{
    [Table("CurrentConfiguration")]
    public record CurrentConfigurationSQLData(
        int Id,
        bool Enabled,
        long InputDate,
        [property: Column("backgroundId")] int BackgroundId) : SQLiteData(Id, Enabled, InputDate)
    {
        public CurrentConfigurationSQLData() : this(0, true, DateTime.Now.Ticks, 0)
        {
        }
    }
}