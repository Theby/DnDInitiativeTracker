using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] DnDITManager dndITManager;
        [SerializeField] CanvasManager canvasManager;

        public static DnDITManager DnDITManager { get; private set; }
        public static CanvasManager CanvasManager { get; private set; }

        void Awake()
        {
            DnDITManager = dndITManager;
            CanvasManager = canvasManager;

            DnDITManager.Initialize();
            CanvasManager.Initialize();
        }
    }
}