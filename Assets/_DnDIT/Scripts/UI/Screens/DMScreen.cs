using System;
using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.Extensions;
using DnDInitiativeTracker.UIData;
using UnityEngine;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class DMScreen : Panel
    {
        [Header("Scroll")]
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] GameObject scrollContent;
        [SerializeField] CharacterInitiativeLayout characterInitiativeLayoutPrefab;

        public event Action<int> OnRemoveLayout;

        DMScreenData _data;
        List<CharacterInitiativeLayout> _layoutList = new();

        public override void Initialize()
        {
            scrollContent.transform.DestroyChildren();
        }

        public void SetData(DMScreenData data)
        {
            _data = data;

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

            var positionIndex = _layoutList.Count + 1;
            var nameList = _data.AllCharacterNames;
            characterInitiativeLayout.Initialize(positionIndex);
            characterInitiativeLayout.SetData(characterUIData, nameList);
            characterInitiativeLayout.OnRemove += OnRemoveLayout;

            _layoutList.Add(characterInitiativeLayout);
        }

        public void AddCharacterInitiativeLayout(CharacterUIData characterUIData)
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
    }
}