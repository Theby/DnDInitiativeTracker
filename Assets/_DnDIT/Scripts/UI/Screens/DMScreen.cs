using System;
using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.Controller;
using DnDInitiativeTracker.GameData;
using UnityEngine;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class DMScreen : Panel
    {
        [Header("Background")]
        [SerializeField] RawImage backgroundImage;
        [Header("Scroll")]
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] GameObject scrollContent;
        [SerializeField] CharacterInitiativeLayout characterInitiativeLayoutPrefab;
        [Header("Buttons")]
        [SerializeField] Button createButton;
        [SerializeField] Button editButton;
        [SerializeField] Button changeBGButton;
        [SerializeField] Button addMoreButton;
        [SerializeField] Button refreshButton;
        [Header("Popups")]
        [SerializeField] ChangeBGPopup changeBGPopup;

        Data _data;
        List<CharacterInitiativeLayout> _layoutList = new();

        public record Data(
            List<CharacterData> CurrentCharacters,
            Dictionary<string, CharacterData> AllCharacters,
            BackgroundData CurrentBackground,
            Dictionary<string, BackgroundData> AllBackgrounds);

        public override void Initialize()
        {
            createButton.onClick.AddListener(OnCreateButtonPressedHandler);
            editButton.onClick.AddListener(OnEditButtonPressedHandler);
            changeBGButton.onClick.AddListener(OnChangeBGButtonPressedHandler);
            addMoreButton.onClick.AddListener(OnAddMoreButtonPressedHandler);
            refreshButton.onClick.AddListener(OnRefreshButtonPressedHandler);
            
            changeBGPopup.Hide();
            changeBGPopup.Initialize();
            changeBGPopup.OnAddNewBackground += OnAddNewBackgroundHandler;
        }

        public void SetData(Data data)
        {
            _data = data;

            _layoutList.Clear();
            foreach (var characterData in _data.CurrentCharacters)
            {
                InstantiateCharacterInitiativeLayout(characterData);
            }
            RefreshCharacterInitiativeLayoutList();
        }

        void InstantiateCharacterInitiativeLayout(CharacterData characterData)
        {
            var characterInitiativeLayout = Instantiate(characterInitiativeLayoutPrefab, scrollContent.transform);

            var positionIndex = _layoutList.Count + 1;
            var nameList = _data.AllCharacters.Keys.ToList();
            characterInitiativeLayout.Initialize(positionIndex);
            characterInitiativeLayout.SetData(characterData, nameList);
            characterInitiativeLayout.OnRemove += OnRemoveLayoutHandler;

            _layoutList.Add(characterInitiativeLayout);
        }

        void AddCharacterInitiativeLayout()
        {
            InstantiateCharacterInitiativeLayout(null);
        }

        void RemoveCharacterInitiativeLayout(int positionIndex)
        {
            var index = positionIndex - 1;
            var layoutToRemove = _layoutList[index];

            _layoutList.RemoveAt(index);
            Destroy(layoutToRemove.gameObject);

            RefreshCharacterInitiativeLayoutList();
        }

        void RefreshCharacterInitiativeLayoutList()
        {
            _layoutList = _layoutList.OrderByDescending(l => l.Initiative).ToList();
            for (var i = 0; i < _layoutList.Count; i++)
            {
                var layout = _layoutList[i];
                layout.transform.SetAsLastSibling();
                layout.PositionIndex = i + 1;
            }
        }

        #region Handlers

        void OnCreateButtonPressedHandler()
        {

        }

        void OnEditButtonPressedHandler()
        {

        }

        void OnChangeBGButtonPressedHandler()
        {
            var nameList = _data.AllBackgrounds.Keys.ToList();

            changeBGPopup.Show();
            changeBGPopup.SetData(_data.CurrentBackground, nameList);
        }

        void OnAddNewBackgroundHandler()
        {
            NativeGalleryController.GetImageFromGallery(OnNewBackgroundHandler);
        }

        void OnNewBackgroundHandler(Texture2D texture2D)
        {
            backgroundImage.texture = texture2D;
        }

        void OnAddMoreButtonPressedHandler()
        {
            AddCharacterInitiativeLayout();
        }

        void OnRefreshButtonPressedHandler()
        {
            RefreshCharacterInitiativeLayoutList();
        }

        void OnRemoveLayoutHandler(int positionIndex)
        {
            RemoveCharacterInitiativeLayout(positionIndex);
        }

        #endregion
    }
}