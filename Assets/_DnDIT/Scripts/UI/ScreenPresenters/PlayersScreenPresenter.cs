using System;
using System.Collections.Generic;
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

        List<CharacterUIData> _data;
        int _round;
        int _roundTurns;
        int _totalTurns;

        public void Initialize()
        {
            playersScreen.Initialize();
        }

        public void Show()
        {
            RefreshData();
            ShowPlayersScreen();
        }

        void ShowPlayersScreen()
        {
            playersScreen.SetData(_data);
            playersScreen.Show();
        }

        public void Hide()
        {
            playersScreen.Hide();
        }

        public void Refresh()
        {
            RefreshData();
            playersScreen.Refresh();
        }

        void RefreshData()
        {
            _data = dataManager.CurrentEncounter;
        }

        void EditEncounter()
        {
            OnEditEncounter?.Invoke();
        }

        void StartEncounter()
        {
            _round = 1;
            _roundTurns = 1;
            _totalTurns = 1;

            UpdateEncounterInfo();

            playersScreen.SetFightState();
            PlayCharacterAudio();
        }

        void SelectNextCharacter()
        {
            _totalTurns++;
            _roundTurns++;

            if (_roundTurns > _data.Count)
            {
                _roundTurns = 1;
                _round++;
            }

            UpdateEncounterInfo();

            playersScreen.SelectNextCharacter();
            PlayCharacterAudio();
        }

        void StopEncounter()
        {
            _round = 1;
            _roundTurns = 0;
            _totalTurns = 0;

            playersScreen.SetReadyState();
            playersScreen.ResetEncounterOrder();
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

        #region Inspector Handlers

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