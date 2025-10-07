using System;
using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public abstract class FromSQLData<T> where T : SQLiteData
    {
        public int SQLId { get; set; }
        public bool Enabled { get; set; }
        public long InputDate { get; set; }

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