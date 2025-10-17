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

        List<CharacterUIData> _data;
        List<CharacterEncounterLayout> _layoutList = new();

        public override void Initialize()
        {
            scrollContent.transform.DestroyChildren();

            editButton.onClick.AddListener(onEdit.Invoke);
            startButton.onClick.AddListener(onStart.Invoke);
            nextButton.onClick.AddListener(onNext.Invoke);
            stopButton.onClick.AddListener(onStop.Invoke);

            SetReadyState();
        }

        public void SetData(List<CharacterUIData> data)
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
                layout.SetData(layout.Data);
            }
        }

        void InstantiateCurrentEncounter()
        {
            _layoutList.Clear();
            scrollContent.transform.DestroyChildren();
            foreach (var characterUIData in _data)
            {
                InstantiateCharacterInitiativeLayout(characterUIData);
            }
        }

        void InstantiateCharacterInitiativeLayout(CharacterUIData characterUIData)
        {
            var characterInitiativeLayout = Instantiate(characterEncounterLayoutPrefab, scrollContent.transform);
            characterInitiativeLayout.SetData(characterUIData);

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

        public AudioClip GetFirstCharacterAudioClip()
        {
            var layout = _layoutList.FirstOrDefault();
            if (layout == null)
                return null;

            var audioList = layout.Data.AudioClips;
            var randomIndex = Random.Range(0, audioList.Count);
            var audioClip = audioList[randomIndex];

            return audioClip;
        }

        public void ResetEncounterOrder()
        {
            for (var i = 0; i < _data.Count; i++)
            {
                var characterUIData = _data[i];
                var layout = _layoutList[i];

                layout.SetData(characterUIData);
            }
        }
    }
}