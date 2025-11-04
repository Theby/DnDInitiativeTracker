using System;
using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.UIData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditCharacterLayout : MonoBehaviour
{
    [SerializeField] Button changeImageButton;
    [SerializeField] RawImage avatarImage;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] List<SelectAudioLayout> audioLayouts;

    public event Action OnChangeImage;
    public event Action<string> OnNameChanged;
    public event Action<int> OnAddNewAudio;
    public event Action<int, string> OnAudioSelectionChanged;
    public event Action<int> OnPlayAudio;

    public void Initialize()
    {
        changeImageButton.onClick.AddListener(ChangeImageHandler);
        nameInputField.onValueChanged.AddListener(OnNameChangedHandler);

        for (var i = 0; i < audioLayouts.Count; i++)
        {
            var layoutIndex = i;
            var audioLayout = audioLayouts[layoutIndex];

            audioLayout.Initialize();
            audioLayout.OnAddNew += () => OnAddNewAudio?.Invoke(layoutIndex);
            audioLayout.OnSelectionChanged += audioName => OnAudioSelectionChanged?.Invoke(layoutIndex, audioName);;
            audioLayout.OnPlayAudio += () => OnPlayAudio?.Invoke(layoutIndex);
        }
    }

    public void SetData(CharacterUIData data, List<string> audioClipNames)
    {
        avatarImage.texture = data.Avatar.AvatarTexture;
        nameInputField.text = string.IsNullOrEmpty(nameInputField.text) ? data.Name : nameInputField.text;

        for (var i = 0; i < audioLayouts.Count; i++)
        {
            var audioClip = data.AudioClips?.ElementAtOrDefault(i);
            var audioClipName = audioClip?.Name ?? string.Empty;

            var selectAudioLayout = audioLayouts[i];
            selectAudioLayout.SetData(audioClipName, audioClipNames);
        }
    }

    void ChangeImageHandler()
    {
        OnChangeImage?.Invoke();
    }

    void OnNameChangedHandler(string input)
    {
        OnNameChanged?.Invoke(input);
    }
}
