using DnDInitiativeTracker.GameData;

namespace DnDInitiativeTracker.UIData
{
    public abstract class MediaAssetUIData<T> where T : UnityEngine.Object
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public MediaAssetType Type { get; set; }
        public T Data { get; set; }

        protected MediaAssetUIData() =>
            (Name, Path, Type, Data) = (string.Empty, string.Empty, MediaAssetType.Unknown, null);

        protected MediaAssetUIData(MediaAssetData assetData, T data)
        {
            Name = assetData?.Name ?? string.Empty;
            Path = assetData?.Path ?? string.Empty;
            Type = assetData?.Type ?? MediaAssetType.Unknown;
            Data = data;
        }

        public MediaAssetData ToMediaAssetData() =>
            new()
            {
                Name = Name,
                Path = Path,
                Type = Type,
            };
    }
}