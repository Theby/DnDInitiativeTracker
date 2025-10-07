using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class CanvasManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] DMScreenManager dmScreenManager;

        public void Initialize()
        {
            dmScreenManager.Initialize();
        }

        public void ShowDMScreen()
        {
            dmScreenManager.Show();
        }
    }
}