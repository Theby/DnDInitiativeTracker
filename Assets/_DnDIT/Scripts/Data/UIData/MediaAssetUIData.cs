using DnDInitiativeTracker.GameData;

namespace DnDInitiativeTracker.UIData
{
    public abstract class MediaAssetUIData<T> where T : UnityEngine.Object
    {
        public string Name
        {
            get => _assetData.Name;
            set => _assetData.Name = value;
        }

        public string Path
        {
            get => _assetData.Path;
            set => _assetData.Path = value;
        }

        public MediaAssetType Type
        {
            get => _assetData.Type;
            set => _assetData.Type = value;
        }

        public T Data { get; protected set; }

        MediaAssetData _assetData;

        protected MediaAssetUIData(MediaAssetData assetData, T data) => (_assetData, Data) = (assetData, data);

        public void UpdateMediaData(MediaAssetData mediaAssetData) => _assetData = mediaAssetData;
        public MediaAssetData ToMediaAssetData() => _assetData;
    }
}