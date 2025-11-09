using System;
using DnDInitiativeTracker.Manager;
using DnDInitiativeTracker.UI;
using DnDInitiativeTracker.UIData;
using UnityEngine;

namespace DnDInitiativeTracker.ScreenManager
{
    public class PlayersScreenPresenter : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] DataManager dataManager;
        [Header("UI")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] PlayersScreen playersScreen;

        public event Action OnEditEncounter;

        readonly PlayerScreenData _data = new();
        int _round;
        int _roundTurns;
        int _totalTurns;
        (CharacterUIData, int) _currentCharacter = default;
        EncounterState _encounterState; //TODO maybe use state machine?

        public void Initialize()
        {
            _encounterState =  EncounterState.None;
            playersScreen.Initialize();
        }

        public void Show()
        {
            RefreshData();
            ShowPlayersScreen();
        }

        void RefreshData()
        {
            _data.CurrentConfigurationUIData = dataManager.CurrentConfigurationUIData;
        }

        void ShowPlayersScreen()
        {
            playersScreen.SetData(_data);
            playersScreen.Show();

            SetEncounterState();
        }

        public void Hide()
        {
            playersScreen.Hide();
        }

        void EditEncounter()
        {
            OnEditEncounter?.Invoke();
        }

        void SetEncounterState()
        {
            switch (_encounterState)
            {
                case EncounterState.None:
                case EncounterState.Ready:
                case EncounterState.Completed:
                    SetReadyEncounter();
                    break;
                case EncounterState.OnGoing:
                    //TODO SelectLastCharacter();
                    SetReadyEncounter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void SetReadyEncounter()
        {
            _encounterState = EncounterState.Ready;

            _round = 1;
            _roundTurns = 0;
            _totalTurns = 0;

            UpdateEncounterInfo();

            playersScreen.SetReadyState();
        }

        void StartEncounter()
        {
            _encounterState = EncounterState.OnGoing;

            _round = 1;
            _roundTurns = 1;
            _totalTurns = 1;

            UpdateEncounterInfo();

            playersScreen.SetFightState();
            PlayCharacterAudio();
            UpdateCurrentCharacter();
        }

        void SelectNextCharacter()
        {
            _totalTurns++;
            _roundTurns++;

            if (_roundTurns > _data.CurrentConfigurationUIData.CurrentEncounter.Count)
            {
                _roundTurns = 1;
                _round++;
            }

            UpdateEncounterInfo();

            playersScreen.SelectNextCharacter();
            PlayCharacterAudio();
            UpdateCurrentCharacter();
        }

        void SelectLastCharacter()
        {
            //TODO

            // var (character, initiative) = _currentCharacter;
            // playersScreen.SelectCharacter(character, initiative);
        }

        void StopEncounter()
        {
            _encounterState = EncounterState.Completed;

            SetReadyEncounter();
            playersScreen.ResetEncounterOrder();
            _currentCharacter = default;
        }

        void UpdateEncounterInfo()
        {
            playersScreen.SetRounds(_round);
            playersScreen.SetRoundTurn(_roundTurns);
            playersScreen.SetTotalTurns(_totalTurns);
        }

        void PlayCharacterAudio()
        {
            var audioClip = playersScreen.GetFirstCharacterAudioClip();
            audioSource.PlayOneShot(audioClip);
        }

        void UpdateCurrentCharacter()
        {
            var currentCharacterLayout = playersScreen.GetCurrentCharacter();
            _currentCharacter = (currentCharacterLayout.Character, currentCharacterLayout.Initiative);
        }

        #region Main Inspector Handlers

        public void EditEncounterButtonInspectorHandler()
        {
            EditEncounter();
        }

        public void StartEncounterButtonInspectorHandler()
        {
            StartEncounter();
        }

        public void SelectNextCharacterButtonInspectorHandler()
        {
            SelectNextCharacter();
        }

        public void StopEncounterButtonInspectorHandler()
        {
            StopEncounter();
        }

        #endregion
    }
}