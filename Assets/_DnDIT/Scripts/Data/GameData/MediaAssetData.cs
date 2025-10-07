using System;
using DnDInitiativeTracker.SQLData;

namespace DnDInitiativeTracker.GameData
{
    public class MediaAssetData : FromSQLData<MediaAssetSQLData>
    {
        public string Name { get; set; }
        public NativeGallery.MediaType Type { get; set; }
        public string Path { get; set; }

        public MediaAssetData() { }

        public MediaAssetData(MediaAssetSQLData sqlData)
            : base(sqlData)
        {
            if (sqlData == null)
                return;

            var type = (NativeGallery.MediaType)Enum.Parse(typeof(NativeGallery.MediaType), sqlData.Type, true);

            Name = sqlData.Name;
            Type = type;
            Path = sqlData.Path;
        }

        public override MediaAssetSQLData ToSQLData()
        {
            return new MediaAssetSQLData(
                SQLId,
                Enabled,
                InputDate,
                Name,
                Type.ToString(),
                Path
            );
        }
    }
}