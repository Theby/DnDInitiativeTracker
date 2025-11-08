using System;
using System.Collections.Generic;
using DnDInitiativeTracker.UIData;
using UnityEngine;
using UnityEngine.UI;

public class SelectAudioLayout : MonoBehaviour
{
    [SerializeField] Button removeLayoutButton;
    [SerializeField] SelectAssetLayout selectAssetLayout;
    [SerializeField] Button playAudioButton;

    public int Index => transform.GetSiblingIndex();

    public event Action<int> OnRemoveLayout;
    public event Action<int> OnAddNew;
    public event Action<int> OnRemoveNewAudio;
    public event Action<int, string> OnSelectionChanged;
    public event Action<AudioClip> OnPlayAudio;

    AudioUIData _loadedUIData;

    public void Initialize()
    {
        removeLayoutButton.onClick.AddListener(RemoveLayoutHandler);

        selectAssetLayout.Initialize();
        selectAssetLayout.OnAddNew += AddNewHandler;
        selectAssetLayout.OnSelectionChanged += ValueChangedHandler;
        selectAssetLayout.OnRemove += RemoveNewAudioHandler;

        playAudioButton.onClick.AddListener(PlayAudioHandler);
    }

    public void SetData(AudioUIData audioUIData, List<string> audioClipNames)
    {
        _loadedUIData = audioUIData;

        selectAssetLayout.SetData(audioUIData.Name, audioClipNames);
    }

    public void Show()
    {
        ShowDropDown(_loadedUIData);
    }

    public void ShowDropDown(AudioUIData audioUIData)
    {
        selectAssetLayout.ShowDropDown();
        _loadedUIData = audioUIData;
    }

    public void ShowNewAudio(AudioUIData audioUIData)
    {
        selectAssetLayout.ShowNewAsset(audioUIData.Name);
        _loadedUIData = audioUIData;
    }

    public void ReselectDropDown()
    {
        selectAssetLayout.ReselectDropDown();
    }

    void RemoveLayoutHandler()
    {
        OnRemoveLayout?.Invoke(Index);
    }

    void AddNewHandler()
    {
        OnAddNew?.Invoke(Index);
    }

    void ValueChangedHandler(string audioName)
    {
        OnSelectionChanged?.Invoke(Index, audioName);
    }

    void RemoveNewAudioHandler()
    {
        OnRemoveNewAudio?.Invoke(Index);
    }

    void PlayAudioHandler()
    {
        OnPlayAudio?.Invoke(_loadedUIData.Data);
    }
}
