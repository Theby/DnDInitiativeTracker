using System;
using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public abstract class FromSQLData
    {
        public abstract int SQLId { get; set; }
        public abstract bool Enabled { get; set; }
        public abstract long InputDate { get; set; }
    }

    public abstract class FromSQLData<T> : FromSQLData
        where T : SQLiteData
    {
        public sealed override int SQLId { get; set; }
        public sealed override bool Enabled { get; set; }
        public sealed override long InputDate { get; set; }

        protected FromSQLData()
        {
            SQLId = 0;
            Enabled = true;
            InputDate = DateTime.Now.Ticks;
        }

        protected FromSQLData(T sqlData)
        {
            SQLId = sqlData?.Id ?? 0;
            Enabled = sqlData?.Enabled ?? true;
            InputDate = sqlData?.InputDate ?? DateTime.Now.Ticks;
        }

        public abstract T ToSQLData();
    }
}