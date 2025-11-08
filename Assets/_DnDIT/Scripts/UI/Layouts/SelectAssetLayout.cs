using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectAssetLayout : MonoBehaviour
{
    [SerializeField] Button addNewButton;
    [SerializeField] TextMeshProUGUI fileNameLabel;
    [SerializeField] TMP_Dropdown nameDropdown;
    [SerializeField] Button removeButton;

    public event Action OnAddNew;
    public event Action<string> OnSelectionChanged;
    public event Action OnRemove;

    public void Initialize()
    {
        addNewButton.onClick.AddListener(AddNewHandler);
        nameDropdown.onValueChanged.AddListener(ValueChangedHandler);
        removeButton.onClick.AddListener(RemoveHandler);
    }

    public void SetData(string currentName, List<string> assetNames)
    {
        nameDropdown.ClearOptions();
        nameDropdown.AddOptions(assetNames);

        var dropDownIndex = nameDropdown.options.FindIndex(x => x.text == currentName);
        dropDownIndex = dropDownIndex == -1 ? 0 : dropDownIndex;
        nameDropdown.SetValueWithoutNotify(dropDownIndex);

        fileNameLabel.text = currentName;
    }

    public void ShowDropDown()
    {
        fileNameLabel.gameObject.SetActive(false);
        removeButton.gameObject.SetActive(false);

        nameDropdown.gameObject.SetActive(true);
    }

    public void ShowNewAsset(string assetName)
    {
        fileNameLabel.gameObject.SetActive(true);
        removeButton.gameObject.SetActive(true);

        nameDropdown.gameObject.SetActive(false);

        fileNameLabel.text = assetName;
    }

    public void ReselectDropDown()
    {
        nameDropdown.onValueChanged.Invoke(nameDropdown.value);
    }

    void AddNewHandler()
    {
        OnAddNew?.Invoke();
    }

    void ValueChangedHandler(int index)
    {
        var assetName = nameDropdown.options[index].text;
        OnSelectionChanged?.Invoke(assetName);
    }

    void RemoveHandler()
    {
        OnRemove?.Invoke();
    }
}
