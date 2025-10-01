using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public class CurrentConfigurationData : FromSQLData<CurrentConfigurationSQLData>
    {
        public BackgroundData Background { get; set; }

        public CurrentConfigurationData(CurrentConfigurationSQLData sqlData, BackgroundData backgroundData)
            : base(sqlData)
        {
            Background = backgroundData;
        }

        public override CurrentConfigurationSQLData ToSQLData()
        {
            return new CurrentConfigurationSQLData(
                SQLId,
                Enabled,
                InputDate,
                Background?.SQLId ?? -1
            );
        }
    }
}