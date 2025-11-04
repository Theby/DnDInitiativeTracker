using DnDInitiativeTracker.UIData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterEncounterLayout : MonoBehaviour
{
    [SerializeField] RawImage avatarImage;
    [SerializeField] TextMeshProUGUI nameLabel;

    public CharacterUIData Data { get; private set; }

    public void SetData(CharacterUIData data)
    {
        Data = data;
        avatarImage.texture = data.Avatar.AvatarTexture;
        nameLabel.text = data.Name;
    }
}
