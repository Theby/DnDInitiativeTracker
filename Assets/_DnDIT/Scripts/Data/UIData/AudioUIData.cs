using DnDInitiativeTracker.GameData;
using UnityEngine;

namespace DnDInitiativeTracker.UIData
{
    public class AudioUIData : MediaAssetUIData<AudioClip>
    {
        public AudioUIData(MediaAssetData assetData, AudioClip data) : base(assetData, data)
        {
        }
    }
}