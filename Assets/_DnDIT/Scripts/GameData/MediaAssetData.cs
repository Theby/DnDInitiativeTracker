using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public class MediaAssetData : FromSQLData<MediaAssetSQLData>
    {
        public string Type { get; set; }
        public string Path { get; set; }

        public MediaAssetData(MediaAssetSQLData sqlData)
            : base(sqlData)
        {
            if (sqlData == null)
                return;

            Type = sqlData.Type;
            Path = sqlData.Path;
        }

        public override MediaAssetSQLData ToSQLData()
        {
            return new MediaAssetSQLData(
                SQLId,
                Enabled,
                InputDate,
                Type,
                Path
            );
        }
    }
}