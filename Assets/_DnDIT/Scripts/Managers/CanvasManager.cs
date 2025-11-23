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
        [SerializeField] public RawImage background;
        [SerializeField] DMScreenPresenter dmScreenPresenter;
        [SerializeField] PlayersScreenPresenter playersScreenPresenter;

        public void Initialize()
        {
            dmScreenPresenter.Initialize();
            dmScreenPresenter.OnGoBack += ShowPlayersScreen;

            playersScreenPresenter.Initialize();
            playersScreenPresenter.OnEditEncounter += ShowDMScreen;

            SetBackground();
        }

        public void ShowPlayersScreen()
        {
            HideScreens();
            playersScreenPresenter.Show();
        }

        public void ShowDMScreen()
        {
            HideScreens();
            dmScreenPresenter.Show();
        }

        void HideScreens()
        {
            playersScreenPresenter.Hide();
            dmScreenPresenter.Hide();
        }

        void SetBackground()
        {
            background.texture = dataManager.CurrentConfigurationUIData.CurrentBackground.Data;
        }
    }
}