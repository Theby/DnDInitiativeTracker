using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectAudioLayout : MonoBehaviour
{
    [SerializeField] Button addNewAudioButton;
    [SerializeField] TMP_Dropdown audioDropdown;
    [SerializeField] Button playAudioButton;

    public event Action OnAddNew;
    public event Action<string> OnSelectionChanged;
    public event Action OnPlayAudio;

    public void Initialize()
    {
        addNewAudioButton.onClick.AddListener(AddNewHandler);
        audioDropdown.onValueChanged.AddListener(ValueChangedHandler);
        playAudioButton.onClick.AddListener(PlayAudioHandler);
    }

    public void SetData(string currentName, List<string> audioClipNames)
    {
        audioDropdown.ClearOptions();
        audioDropdown.AddOptions(audioClipNames);

        var dropDownIndex = audioClipNames.FindIndex(x => x == currentName);
        dropDownIndex = Mathf.Max(dropDownIndex, 0);
        audioDropdown.value = dropDownIndex;
    }

    void AddNewHandler()
    {
        OnAddNew?.Invoke();
    }

    void ValueChangedHandler(int index)
    {
        var audioName = audioDropdown.options[index].text;
        OnSelectionChanged?.Invoke(audioName);
    }

    void PlayAudioHandler()
    {
        OnPlayAudio?.Invoke();
    }
}
