using System.Collections.Generic;
using System.Linq;
using DnDInitiativeTracker.Extensions;
using DnDInitiativeTracker.UIData;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DnDInitiativeTracker.UI
{
    public class PlayersScreen : Panel
    {
        [Header("UI")]
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] GameObject scrollContent;
        [SerializeField] CharacterEncounterLayout characterEncounterLayoutPrefab;
        [SerializeField] TextMeshProUGUI roundsLabel;
        [SerializeField] TextMeshProUGUI roundTurnLabel;
        [SerializeField] TextMeshProUGUI totalTurnsLabel;
        [SerializeField] Button editButton;
        [SerializeField] Button startButton;
        [SerializeField] Button nextButton;
        [SerializeField] Button stopButton;
        [Header("Events")]
        [SerializeField] UnityEvent onEdit;
        [SerializeField] UnityEvent onStart;
        [SerializeField] UnityEvent onNext;
        [SerializeField] UnityEvent onStop;

        PlayerScreenData _data;
        List<CharacterEncounterLayout> _layoutList = new();

        public override void Initialize()
        {
            editButton.onClick.AddListener(onEdit.Invoke);
            startButton.onClick.AddListener(onStart.Invoke);
            nextButton.onClick.AddListener(onNext.Invoke);
            stopButton.onClick.AddListener(onStop.Invoke);
        }

        public void SetData(PlayerScreenData data)
        {
            _data = data;
            InstantiateCurrentEncounter();
        }

        void InstantiateCurrentEncounter()
        {
            _layoutList.Clear();
            scrollContent.transform.DestroyChildren();
            for (var i = 0; i < _data.CurrentConfigurationUIData.CurrentEncounter.Count; i++)
            {
                var characterUIData = _data.CurrentConfigurationUIData.CurrentEncounter[i];
                var initiative = _data.CurrentConfigurationUIData.InitiativeList[i];

                InstantiateCharacterInitiativeLayout(characterUIData, initiative);
            }
        }

        void InstantiateCharacterInitiativeLayout(CharacterUIData characterUIData, int initiative)
        {
            var characterInitiativeLayout = Instantiate(characterEncounterLayoutPrefab, scrollContent.transform);
            characterInitiativeLayout.SetData(characterUIData, initiative);

            _layoutList.Add(characterInitiativeLayout);
        }

        public void SetRounds(int rounds)
        {
            roundsLabel.text = $"{rounds}";
        }

        public void SetRoundTurn(int turn)
        {
            roundTurnLabel.text = $"{turn}";
        }

        public void SetTotalTurns(int turns)
        {
            totalTurnsLabel.text = $"{turns}";
        }

        public void SetReadyState()
        {
            SetRounds(1);
            SetRoundTurn(0);
            SetTotalTurns(0);

            startButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(false);

            scrollRect.verticalNormalizedPosition = 1.0f;
        }

        public void SetFightState()
        {
            startButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(true);

            scrollRect.verticalNormalizedPosition = 1.0f;
        }

        public void SelectNextCharacter()
        {
            var layout = _layoutList.FirstOrDefault();
            if (layout == null)
                return;

            _layoutList.Add(layout);
            _layoutList.RemoveAt(0);

            layout.transform.SetAsLastSibling();
            scrollRect.verticalNormalizedPosition = 1.0f;
        }

        public void SelectCharacter(CharacterUIData character, int initiative)
        {
            //TODO not liking this logic

            // var characterLayout = _layoutList.FirstOrDefault(l => l.Character.Name == character.Name);
            // characterLayout ??= _layoutList.FirstOrDefault(l => l.Initiative <= initiative);
            // characterLayout ??= _layoutList.Last();
            //
            // var newLayoutList = new List<CharacterEncounterLayout>(_layoutList);
            // var layoutIndex = _layoutList.IndexOf(characterLayout);
            // for (int i = 0; i < layoutIndex; i++)
            // {
            //     var layout = _layoutList[i];
            //     layout.transform.SetAsLastSibling();
            //
            //     newLayoutList.Add(layout);
            //     newLayoutList.RemoveAt(0);
            // }
            //
            // _layoutList = newLayoutList;
            // scrollRect.verticalNormalizedPosition = 1.0f;
        }

        public AudioClip GetFirstCharacterAudioClip()
        {
            var layout = _layoutList.FirstOrDefault();
            if (layout == null)
                return null;

            var audioList = layout.Character.AudioList;
            var randomIndex = Random.Range(0, audioList.Count);
            var audioClip = audioList[randomIndex];

            return audioClip.Data;
        }

        public void ResetEncounterOrder()
        {
            for (var i = 0; i < _layoutList.Count; i++)
            {
                var layout = _layoutList[i];
                var characterUIData = _data.CurrentConfigurationUIData.CurrentEncounter[i];
                var initiative = _data.CurrentConfigurationUIData.InitiativeList[i];

                layout.SetData(characterUIData, initiative);
            }
        }

        public CharacterEncounterLayout GetCurrentCharacter()
        {
            return _layoutList.First();
        }
    }
}