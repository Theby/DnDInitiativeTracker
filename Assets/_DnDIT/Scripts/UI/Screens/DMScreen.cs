using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class DMScreen : CanvasScreen
    {
        [Header("Background")]
        [SerializeField] Image backgroundImage;
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

        List<CharacterInitiativeLayout> _layoutList = new();

        public override void Initialize()
        {
            createButton.onClick.AddListener(OnCreateButtonPressedHandler);
            editButton.onClick.AddListener(OnEditButtonPressedHandler);
            changeBGButton.onClick.AddListener(OnChangeBGButtonPressedHandler);
            addMoreButton.onClick.AddListener(OnAddMoreButtonPressedHandler);
            refreshButton.onClick.AddListener(OnRefreshButtonPressedHandler);
        }

        void AddCharacterInitiativeLayout()
        {
            var characterInitiativeLayout = Instantiate(characterInitiativeLayoutPrefab, scrollContent.transform);
            _layoutList.Add(characterInitiativeLayout);

            var positionIndex = _layoutList.Count;
            characterInitiativeLayout.Initialize(positionIndex);
            characterInitiativeLayout.OnRemove += OnRemoveLayoutHandler;
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

        void ChangeBG()
        {
            //GameManager.DnDITManager.GetImageFromGallery(t => Debug.Log(t.name));
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
            ChangeBG();
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