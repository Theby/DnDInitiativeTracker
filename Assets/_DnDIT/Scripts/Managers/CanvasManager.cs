using System.Collections.Generic;
using DnDInitiativeTracker.GameData;
using DnDInitiativeTracker.UI;
using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class CanvasManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] PlayersScreen playersScreen;
        [SerializeField] DMScreen dmScreen;

        void Awake()
        {
            HidePanels();
        }

        public void Initialize()
        {
            playersScreen.Initialize();

            dmScreen.Initialize();
        }

        void HidePanels()
        {
            playersScreen.Hide();
            dmScreen.Hide();
        }

        public void ShowPlayerScreen()
        {

        }

        public void ShowDMScreen(DMScreen.Data data)
        {
            HidePanels();
            dmScreen.Show();
            dmScreen.SetData(data);
        }
    }
}