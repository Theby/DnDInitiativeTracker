using DnDInitiativeTracker.UI;
using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class CanvasManager : MonoBehaviour
    {
        [SerializeField] DMScreen dmScreen;

        public void Initialize()
        {
            dmScreen.Initialize();
        }
    }
}