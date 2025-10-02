using DnDInitiativeTracker.UI;
using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class CanvasManager : MonoBehaviour
    {
        [SerializeField] PlayersScreen playersScreen;
        [SerializeField] DMScreen dmScreen;

        void Awake()
        {
            HideScreens();
        }

        public void Initialize()
        {
            //ShowPlayerScreen();
            ShowDMScreen();
        }

        void HideScreens()
        {
            playersScreen.Hide();
            dmScreen.Hide();
        }

        void ShowScreen(CanvasScreen screen)
        {
            HideScreens();
            screen.Show();
            screen.Initialize();
        }

        public void ShowPlayerScreen()
        {
            ShowScreen(playersScreen);
        }

        public void ShowDMScreen()
        {
            ShowScreen(dmScreen);
        }
    }
}