using DnDInitiativeTracker.Manager;
using DnDInitiativeTracker.UI;
using DnDInitiativeTracker.UIData;
using UnityEngine;
using UnityEngine.UI;

public class DMScreenManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] DataManager dataManager;
    [Header("UI")]
    [SerializeField] RawImage background;
    [SerializeField] DMScreen dmScreen;
    [SerializeField] CreateCharacterPopup createCharacterPopup;
    [SerializeField] EditCharacterPopup editCharacterPopup;
    [SerializeField] ChangeBGPopup changeBGPopup;

    DMScreenData _data = new();

    public void Initialize()
    {
        dmScreen.Initialize();
        dmScreen.OnRemoveLayout += RemoveCharacterLayoutHandler;

        changeBGPopup.Initialize();

        dataManager.OnDataUpdated += Refresh;
    }

    public void Show()
    {
        HidePopups();

        RefreshData();

        dmScreen.SetData(_data);
        dmScreen.Show();
    }

    void ShowChangeBGPopup()
    {
        changeBGPopup.SetData(_data);
        changeBGPopup.Show();
    }

    void ShowCreateCharacterPopup()
    {
        changeBGPopup.SetData(_data);
        changeBGPopup.Show();
    }

    void ShowEditCharacterPopup()
    {
        changeBGPopup.SetData(_data);
        changeBGPopup.Show();
    }

    public void Hide()
    {
        dmScreen.Hide();
        HidePopups();
    }

    void HidePopups()
    {
        changeBGPopup.Hide();
    }

    void Refresh()
    {
        RefreshData();
        RefreshBackground();

        if (changeBGPopup.gameObject.activeSelf)
        {
            changeBGPopup.Refresh();
        }
    }

    void RefreshData()
    {
        _data.CurrentEncounter = dataManager.CurrentEncounter;
        _data.CharacterNames = dataManager.GetAllCharacterNames();
        _data.CurrentBackground = dataManager.CurrentBackground;
        _data.BackgroundNames = dataManager.GetAllBackgroundNames();
    }

    void RefreshBackground()
    {
        background.texture = dataManager.CurrentBackground.BackgroundTexture;
    }

    #region Inspector Handlers

    public void CreateCharacterButtonInspectorHandler()
    {
        ShowCreateCharacterPopup();
    }

    public void EditCharacterButtonInspectorHandler()
    {
        ShowEditCharacterPopup();
    }

    public void ChangeBGButtonInspectorHandler()
    {
        ShowChangeBGPopup();
    }

    public void AddMoreButtonInspectorHandler()
    {
        //dmScreen.AddCharacterInitiativeLayout(null); //pass default character
    }

    public void RefreshButtonInspectorHandler()
    {
        //dmScreen.RefreshCharacterInitiativeLayoutList();
    }

    public void BackgroundSelectionDropdownInspectorHandler(int index)
    {
        if (_data.BackgroundNames.Count <= index)
            return;

        var bgName = _data.BackgroundNames[index];
        if (_data.CurrentBackground.Name == bgName)
            return;

        dataManager.UpdateCurrentBackground(bgName);
    }

    public void AddNewBackgroundButtonInspectorHandler()
    {
        //TODO show loader and stop on complete
        dataManager.TryCreateNewBackground();
    }

    #endregion

    #region Handlers

    void RemoveCharacterLayoutHandler(int positionIndex)
    {
        dmScreen.RemoveCharacterInitiativeLayout(positionIndex);
    }

    #endregion
}
