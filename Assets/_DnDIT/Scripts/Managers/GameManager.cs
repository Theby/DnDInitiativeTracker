using System;
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

        async void Initialize()
        {
            try
            {
                await NativeBrowserController.RequestPermissionsAsync();
                await dataManager.InitializeAsync();

                canvasManager.Initialize();

                canvasManager.ShowPlayersScreen();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}