using System;
using System.Collections.Generic;
using DnDInitiativeTracker.Extensions;
using DnDInitiativeTracker.UIData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditCharacterLayout : MonoBehaviour
{
    [SerializeField] SelectAssetLayout selectAvatarLayout;
    [SerializeField] RawImage previewAvatar;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] Button addAudioLayoutButton;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject scrollContent;
    [SerializeField] SelectAudioLayout selectAudioLayoutPrefab;

    public CharacterUIData EditCharacter { get; private set; }

    public event Action OnAddNewAvatar;
    public event Action<string> OnAvatarSelectionChanged;
    public event Action OnAvatarRemoved;
    public event Action OnAddAudioLayout;
    public event Action<int> OnAudioLayoutRemoved;
    public event Action<int> OnAddNewAudio;
    public event Action<int, string> OnAudioSelectionChanged;
    public event Action<int> OnAudioRemoved;
    public event Action<AudioClip> OnPlayAudio;

    DMScreenData _data;

    readonly List<SelectAudioLayout> _layoutList = new();

    public void Initialize()
    {
        selectAvatarLayout.Initialize();
        selectAvatarLayout.OnAddNew += AddNewAvatarHandler;
        selectAvatarLayout.OnSelectionChanged += AvatarSelectionChangedHandler;
        selectAvatarLayout.OnRemove += AvatarRemovedHandler;

        nameInputField.text = string.Empty;
        nameInputField.onValueChanged.AddListener(NameChangedHandler);

        addAudioLayoutButton.onClick.AddListener(AddAudioLayoutHandler);
    }

    public void SetData(DMScreenData data, CharacterUIData editCharacter)
    {
        _data = data;
        EditCharacter = editCharacter;

        selectAvatarLayout.SetData(editCharacter.Avatar.Name, _data.AvatarNames);
        nameInputField.text = EditCharacter.Name;

        _layoutList.Clear();
        scrollContent.transform.DestroyChildren();
        foreach (var audioUIData in EditCharacter.AudioList)
        {
            InstantiateSelectAudioLayout(audioUIData);
        }
    }

    public void Show()
    {
        selectAvatarLayout.ShowDropDown();
    }

    public void Refresh()
    {
        previewAvatar.texture = EditCharacter.Avatar.Data;
    }

    public void ShowNewAvatar(TextureUIData avatarUIData)
    {
        selectAvatarLayout.ShowNewAsset(avatarUIData.Name);

        EditCharacter.Avatar = avatarUIData;
        Refresh();
    }

    public void ShowAvatarDropdown(TextureUIData avatarUIData)
    {
        selectAvatarLayout.ShowDropDown();

        EditCharacter.Avatar = avatarUIData;
        Refresh();
    }

    public void ReselectAvatarDropDown()
    {
        selectAvatarLayout.ReselectDropDown();
    }

    public void AddAudioLayout(AudioUIData audioUIData)
    {
        InstantiateSelectAudioLayout(audioUIData);
        EditCharacter.AudioList.Add(audioUIData);
    }

    void InstantiateSelectAudioLayout(AudioUIData audioUIData)
    {
        var selectAudioLayout = Instantiate(selectAudioLayoutPrefab, scrollContent.transform);

        selectAudioLayout.Initialize();
        selectAudioLayout.SetData(audioUIData, _data.AudioNames);
        selectAudioLayout.OnRemoveLayout += AudioLayoutRemovedHandler;
        selectAudioLayout.OnAddNew += AddNewAudioHandler;
        selectAudioLayout.OnSelectionChanged += AudioSelectionChangedHandler;
        selectAudioLayout.OnRemoveNewAudio += AudioRemovedHandler;
        selectAudioLayout.OnPlayAudio += PlayAudioHandler;

        selectAudioLayout.Show();

        _layoutList.Add(selectAudioLayout);
    }

    public void RemoveAudioLayout(int index)
    {
        var layoutToRemove = _layoutList[index];

        _layoutList.RemoveAt(index);
        Destroy(layoutToRemove.gameObject);

        EditCharacter.AudioList.RemoveAt(index);
    }

    public void ShowNewAudio(int index, AudioUIData audioUIData)
    {
        var audioLayout = _layoutList[index];
        audioLayout.ShowNewAudio(audioUIData);

        EditCharacter.AudioList[index] = audioUIData;
    }

    public void ShowAudioDropdown(int index, AudioUIData audioUIData)
    {
        var audioLayout = _layoutList[index];
        audioLayout.ShowDropDown(audioUIData);

        EditCharacter.AudioList[index] = audioUIData;
    }

    public void ReselectAudioDropDown(int index)
    {
        var audioLayout = _layoutList[index];
        audioLayout.ReselectDropDown();
    }

    void AudioLayoutRemovedHandler(int index)
    {
        OnAudioLayoutRemoved?.Invoke(index);
    }

    void AddNewAudioHandler(int index)
    {
        OnAddNewAudio?.Invoke(index);
    }

    void AudioSelectionChangedHandler(int index, string audioName)
    {
        OnAudioSelectionChanged?.Invoke(index, audioName);
    }

    void AudioRemovedHandler(int index)
    {
        OnAudioRemoved?.Invoke(index);
    }

    void PlayAudioHandler(AudioClip audioClip)
    {
        OnPlayAudio?.Invoke(audioClip);
    }

    void AddNewAvatarHandler()
    {
        OnAddNewAvatar?.Invoke();
    }

    void AvatarSelectionChangedHandler(string avatarName)
    {
        OnAvatarSelectionChanged?.Invoke(avatarName);
    }

    void AvatarRemovedHandler()
    {
        OnAvatarRemoved?.Invoke();
    }

    void NameChangedHandler(string input)
    {
        EditCharacter.Name = input;
    }

    void AddAudioLayoutHandler()
    {
        OnAddAudioLayout?.Invoke();
    }
}
