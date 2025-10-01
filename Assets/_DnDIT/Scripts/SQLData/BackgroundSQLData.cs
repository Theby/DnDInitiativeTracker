using System;
using SQLite;

namespace DnDInitiativeTracker.SQLData
{
    [Table("Background")]
    public record BackgroundSQLData(
        int Id,
        bool Enabled,
        long InputDate,
        [property: Column("mediaAssetId")] int MediaAssetId) : SQLiteData(Id, Enabled, InputDate)
    {
        public BackgroundSQLData() : this(0, true, DateTime.Now.Ticks, 0)
        {
        }
    }
}