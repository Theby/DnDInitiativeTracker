using System;
using SQLite;

namespace DnDInitiativeTracker.SQLData
{
    [Table("AssetDirectory")]
    public record MediaAssetSQLData(
        int Id,
        bool Enabled,
        long InputDate,
        [property: Column("name")] string Name,
        [property: Column("type"), Indexed] int Type,
        [property: Column("path")] string Path) : SQLiteData(Id, Enabled, InputDate)
    {
        public MediaAssetSQLData() : this(0, true, DateTime.Now.Ticks, string.Empty, 0, string.Empty)
        {
        }
    }
}