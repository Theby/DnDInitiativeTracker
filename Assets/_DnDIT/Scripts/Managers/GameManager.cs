using System.Collections;
using DnDInitiativeTracker.Controller;
using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] DataManager dataManager;
        [SerializeField] CanvasManager canvasManager;

        IEnumerator Start()
        {
            yield return NativeGalleryController.RequestPermissions();

            yield return dataManager.Initialize();

            canvasManager.Initialize();

            canvasManager.ShowDMScreen();
        }
    }
}