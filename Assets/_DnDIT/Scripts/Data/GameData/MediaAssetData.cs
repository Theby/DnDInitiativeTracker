using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public class MediaAssetData : FromSQLData<MediaAssetSQLData>
    {
        public string Name { get; set; }
        public MediaAssetType Type { get; set; }
        public string Path { get; set; }

        public MediaAssetData() { }

        public MediaAssetData(MediaAssetSQLData sqlData, string name, MediaAssetType type, string path)
            : base(sqlData)
        {
            Name = name;
            Type = type;
            Path = path;
        }

        public override MediaAssetSQLData ToSQLData()
        {
            return new MediaAssetSQLData(
                SQLId,
                Enabled,
                InputDate,
                Name,
                (int)Type,
                Path
            );
        }
    }
}