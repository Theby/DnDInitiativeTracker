using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.Extensions;
using DnDInitiativeTracker.UIData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class DMScreen : Panel
    {
        [Header("UI")]
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] GameObject scrollContent;
        [SerializeField] CharacterInitiativeLayout characterInitiativeLayoutPrefab;
        [SerializeField] Button addMoreButton;
        [SerializeField] Button refreshButton;
        [SerializeField] Button createCharacterButton;
        [SerializeField] Button editCharacterButton;
        [SerializeField] Button changeBGButton;
        [Header("UI")]
        [SerializeField] UnityEvent onAddMore;
        [SerializeField] UnityEvent onRefresh;
        [SerializeField] UnityEvent<int, string> onCharacterSelected;
        [SerializeField] UnityEvent<int> onRemoveLayout;
        [SerializeField] UnityEvent onCreateCharacter;
        [SerializeField] UnityEvent onEditCharacter;
        [SerializeField] UnityEvent onChangeBG;

        DMScreenData _data;
        List<CharacterInitiativeLayout> _layoutList = new();

        public override void Initialize()
        {
            scrollContent.transform.DestroyChildren();

            addMoreButton.onClick.AddListener(onAddMore.Invoke);
            refreshButton.onClick.AddListener(onRefresh.Invoke);
            createCharacterButton.onClick.AddListener(onCreateCharacter.Invoke);
            editCharacterButton.onClick.AddListener(onEditCharacter.Invoke);
            changeBGButton.onClick.AddListener(onChangeBG.Invoke);
        }

        public void SetData(DMScreenData data)
        {
            _data = data;
            InstantiateCurrentEncounter();
        }

        public override void Show()
        {
            base.Show();

            Refresh();
        }

        public void Refresh()
        {
            foreach (var layout in _layoutList)
            {
                layout.SetData(layout.Data, _data.CharacterNames);
            }
        }

        public void RefreshLayout(int layoutIndex, CharacterUIData characterUIData)
        {
            var layout = _layoutList[layoutIndex];
            layout.SetData(characterUIData, _data.CharacterNames);
        }

        void InstantiateCurrentEncounter()
        {
            _layoutList.Clear();
            scrollContent.transform.DestroyChildren();
            foreach (var characterData in _data.CurrentEncounter)
            {
                InstantiateCharacterInitiativeLayout(characterData);
            }
            RefreshCharacterInitiativeLayoutList();
        }

        void InstantiateCharacterInitiativeLayout(CharacterUIData characterUIData)
        {
            var characterInitiativeLayout = Instantiate(characterInitiativeLayoutPrefab, scrollContent.transform);

            characterInitiativeLayout.Initialize();
            characterInitiativeLayout.SetData(characterUIData, _data.CharacterNames);
            characterInitiativeLayout.OnSelectionChanged += onCharacterSelected.Invoke;
            characterInitiativeLayout.OnRemove += onRemoveLayout.Invoke;
            characterInitiativeLayout.PositionIndex = _layoutList.Count + 1;

            _layoutList.Add(characterInitiativeLayout);
        }

        public void AddCharacterInitiativeLayout(CharacterUIData characterUIData = null)
        {
            InstantiateCharacterInitiativeLayout(characterUIData);
        }

        public void RemoveCharacterInitiativeLayout(int positionIndex)
        {
            var index = positionIndex - 1;
            var layoutToRemove = _layoutList[index];

            _layoutList.RemoveAt(index);
            Destroy(layoutToRemove.gameObject);

            RefreshCharacterInitiativeLayoutList();
        }

        public void RefreshCharacterInitiativeLayoutList()
        {
            _layoutList = _layoutList.OrderByDescending(l => l.Initiative).ToList();
            for (var i = 0; i < _layoutList.Count; i++)
            {
                var layout = _layoutList[i];
                layout.transform.SetAsLastSibling();
                layout.PositionIndex = i + 1;
            }
        }

        public List<CharacterUIData> GetEncounter()
        {
            return _layoutList.Where(l => l.Data != null)
                .Select(l => l.Data)
                .ToList();
        }
    }
}