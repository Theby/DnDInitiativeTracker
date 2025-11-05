using DnDInitiativeTracker.GameData;
using UnityEngine;

namespace DnDInitiativeTracker.UIData
{
    public class TextureUIData : MediaAssetUIData<Texture>
    {
        public TextureUIData(MediaAssetData assetData, Texture data) : base(assetData, data)
        {
        }
    }
}