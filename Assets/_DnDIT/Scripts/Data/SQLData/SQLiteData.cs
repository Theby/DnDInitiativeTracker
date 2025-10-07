using System;
using SQLite;

namespace DnDInitiativeTracker.SQLData
{
    public record SQLiteData(
        [property: PrimaryKey, AutoIncrement, Column("id"), Indexed] int Id,
        [property: Column("enabled")] bool Enabled,
        [property: Column("input_date")] long InputDate)
    {
        public SQLiteData() : this(0, true, DateTime.Now.Ticks)
        {
        }
    }
}