using System;
using System.Linq;
using SQLite;
using UnityEngine;

namespace DnDInitiativeTracker.SQLData
{
    [Table("CurrentConfiguration")]
    public record CurrentConfigurationSQLData(
        int Id,
        bool Enabled,
        long InputDate,
        [property: Column("characterIds")] string CharacterIds,
        [property: Column("backgroundId")] int BackgroundId) : SQLiteData(Id, Enabled, InputDate)
    {

        int[] _characterIdList = null;
        public int[] CharacterIdList => _characterIdList ??= GetCharacterIds();

        public CurrentConfigurationSQLData() : this(0, true, DateTime.Now.Ticks, "0", 0)
        {
        }

        int[] GetCharacterIds()
        {
            if (string.IsNullOrEmpty(CharacterIds))
                return Array.Empty<int>();

            var splitIds = CharacterIds.Split(',');
            return splitIds.Select(int.Parse).ToArray();
        }
    }
}