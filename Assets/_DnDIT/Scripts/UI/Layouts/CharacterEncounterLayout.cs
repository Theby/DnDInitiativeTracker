using DnDInitiativeTracker.UIData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterEncounterLayout : MonoBehaviour
{
    [SerializeField] RawImage avatarImage;
    [SerializeField] TextMeshProUGUI nameLabel;

    public CharacterUIData Character { get; private set; }
    public int Initiative { get; private set; }

    public void SetData(CharacterUIData character, int initiative)
    {
        Character = character;
        Initiative = initiative;
        
        avatarImage.texture = character.Avatar.Data;
        nameLabel.text = character.Name;
    }
}
