using DnDInitiativeTracker.ScreenManager;
using UnityEngine;
using UnityEngine.UI;

namespace DnDInitiativeTracker.Manager
{
    public class CanvasManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] DataManager dataManager;
        [Header("UI")]
        [SerializeField] RawImage background;
        [SerializeField] DMScreenManager dmScreenManager;
        [SerializeField] PlayersScreenManager playersScreenManager;

        public void Initialize()
        {
            dmScreenManager.Initialize();
            dmScreenManager.OnGoBack += ShowPlayersScreen;

            playersScreenManager.Initialize();
            playersScreenManager.OnEditEncounter += ShowDMScreen;

            SetBackground();
        }

        public void ShowPlayersScreen()
        {
            HideScreens();
            playersScreenManager.Show();
        }

        public void ShowDMScreen()
        {
            HideScreens();
            dmScreenManager.Show();
        }

        void HideScreens()
        {
            playersScreenManager.Hide();
            dmScreenManager.Hide();
        }

        void SetBackground()
        {
            background.texture = dataManager.CurrentBackground.BackgroundTexture;
        }
    }
}