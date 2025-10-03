using System.Collections;
using System.Collections.Generic;
using DnDInitiativeTracker.Controller;
using DnDInitiativeTracker.GameData;
using DnDInitiativeTracker.UI;
using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] DnDITManager dndITManager;
        [SerializeField] CanvasManager canvasManager;

        public static DnDITManager DnDITManager { get; private set; }
        public static CanvasManager CanvasManager { get; private set; }

        List<CharacterData> _currentCharacters = new List<CharacterData>();
        BackgroundData _currentBackground;

        void Awake()
        {
            DnDITManager = dndITManager;
            CanvasManager = canvasManager;
        }

        IEnumerator Start()
        {
            yield return NativeGalleryController.RequestPermissions();

            DnDITManager.Initialize();
            CanvasManager.Initialize();

            Test();
        }

        void Test()
        {
            var allCharacters = DnDITManager.GetAllCharacters();
            var allBackgrounds = DnDITManager.GetAllBackgrounds();
            var data = new DMScreen.Data(_currentCharacters, allCharacters, _currentBackground, allBackgrounds);

            CanvasManager.ShowDMScreen(data);
        }
    }
}