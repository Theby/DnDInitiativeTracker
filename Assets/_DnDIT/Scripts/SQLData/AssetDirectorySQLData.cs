using System;
using SQLite;

namespace DnDInitiativeTracker.SQLData
{
    [Table("AssetDirectory")]
    public record AssetDirectorySQLData(
        int Id,
        bool Enabled,
        long InputDate,
        [property: Column("path")] string Path) : SQLiteData(Id, Enabled, InputDate)
    {
        public AssetDirectorySQLData() : this(0, true, DateTime.Now.Ticks, string.Empty)
        {
        }
    }
}