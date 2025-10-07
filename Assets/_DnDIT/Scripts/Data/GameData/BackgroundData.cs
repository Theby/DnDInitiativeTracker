using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public class BackgroundData : FromSQLData<BackgroundSQLData>
    {
        public MediaAssetData MediaAssetData { get; set; }

        public BackgroundData() { }

        public BackgroundData(BackgroundSQLData sqlData, MediaAssetData mediaAssetData)
            : base(sqlData)
        {
            MediaAssetData = mediaAssetData;
        }

        public override BackgroundSQLData ToSQLData()
        {
            return new BackgroundSQLData(
                SQLId,
                Enabled,
                InputDate,
                MediaAssetData?.SQLId ?? -1
            );
        }
    }
}