using System.Collections.Generic;
using System.Threading.Tasks;
using DnDInitiativeTracker.Controller;
using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] DataManager dataManager;
        [SerializeField] CanvasManager canvasManager;

        void Start()
        {
            Initialize();
        }

        async Task Initialize()
        {
            await NativeBrowserController.RequestPermissionsAsync();
            await dataManager.InitializeAsync();

            canvasManager.Initialize();

            canvasManager.ShowPlayersScreen();
        }
    }
}