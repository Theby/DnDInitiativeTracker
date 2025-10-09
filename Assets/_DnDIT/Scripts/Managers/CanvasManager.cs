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

        public void Initialize()
        {
            dmScreenManager.Initialize();
            SetBackground();
        }

        public void ShowDMScreen()
        {
            HideScreens();
            dmScreenManager.Show();
        }

        void HideScreens()
        {
            dmScreenManager.Hide();
        }

        void SetBackground()
        {
            background.texture = dataManager.CurrentBackground.BackgroundTexture;
        }
    }
}