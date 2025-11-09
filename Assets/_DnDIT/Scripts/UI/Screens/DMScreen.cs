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
        [SerializeField] Button backButton;
        [Header("Events")]
        [SerializeField] UnityEvent onAddMore;
        [SerializeField] UnityEvent onRefresh;
        [SerializeField] UnityEvent<int, string> onCharacterSelected;
        [SerializeField] UnityEvent<int> onRemoveLayout;
        [SerializeField] UnityEvent onCreateCharacter;
        [SerializeField] UnityEvent onEditCharacter;
        [SerializeField] UnityEvent onChangeBG;
        [SerializeField] UnityEvent onBack;

        DMScreenData _data;
        List<CharacterInitiativeLayout> _layoutList = new();

        public override void Initialize()
        {
            addMoreButton.onClick.AddListener(onAddMore.Invoke);
            refreshButton.onClick.AddListener(onRefresh.Invoke);
            createCharacterButton.onClick.AddListener(onCreateCharacter.Invoke);
            editCharacterButton.onClick.AddListener(onEditCharacter.Invoke);
            changeBGButton.onClick.AddListener(onChangeBG.Invoke);
            backButton.onClick.AddListener(onBack.Invoke);
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
            for (var i = 0; i < _layoutList.Count; i++)
            {
                var layout = _layoutList[i];
                layout.PositionIndex = i + 1;
            }
        }

        void InstantiateCurrentEncounter()
        {
            _layoutList.Clear();
            scrollContent.transform.DestroyChildren();
            for (var i = 0; i < _data.CurrentConfigurationUIData.CurrentEncounter.Count; i++)
            {
                var characterData = _data.CurrentConfigurationUIData.CurrentEncounter[i];
                var initiative = _data.CurrentConfigurationUIData.InitiativeList[i];
                InstantiateCharacterInitiativeLayout(characterData, initiative);
            }
        }

        void InstantiateCharacterInitiativeLayout(CharacterUIData characterUIData, int initiative)
        {
            var characterInitiativeLayout = Instantiate(characterInitiativeLayoutPrefab, scrollContent.transform);
            var positionIndex = _layoutList.Count + 1;

            characterInitiativeLayout.Initialize();
            characterInitiativeLayout.SetData(positionIndex, characterUIData, initiative, _data.CharacterNames);
            characterInitiativeLayout.OnRemove += onRemoveLayout.Invoke;
            characterInitiativeLayout.OnSelectionChanged += onCharacterSelected.Invoke;

            _layoutList.Add(characterInitiativeLayout);
        }

        public void AddCharacterInitiativeLayout(CharacterUIData characterUIData, int initiative)
        {
            InstantiateCharacterInitiativeLayout(characterUIData, initiative);
        }

        public void RemoveCharacterInitiativeLayout(int index)
        {
            var layoutToRemove = _layoutList[index];

            _layoutList.RemoveAt(index);
            Destroy(layoutToRemove.gameObject);

            Refresh();
        }

        public void UpdateCharacter(int layoutIndex, CharacterUIData characterUIData)
        {
            var layout = _layoutList[layoutIndex];
            layout.UpdateCharacter(characterUIData);
        }

        public void RefreshEncounterOrder()
        {
            _layoutList = _layoutList.OrderByDescending(l => l.Initiative).ToList();
            foreach (var layout in _layoutList)
            {
                layout.transform.SetAsLastSibling();
            }

            Refresh();
        }

        public (List<CharacterUIData>, List<int>) GetEncounter()
        {
            return (_layoutList.Select(l => l.LoadedCharacter).ToList(),
                _layoutList.Select(l => l.Initiative).ToList());
        }
    }
}