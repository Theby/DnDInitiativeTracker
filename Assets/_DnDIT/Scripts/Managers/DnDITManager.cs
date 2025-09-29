using DnDInitiativeTracker.Controller;
using UnityEngine;

namespace DnDInitiativeTracker.Manager
{
    public class DnDITManager : MonoBehaviour
    {
        SQLController _sqlController;

        public void Initialize()
        {
            _sqlController = new SQLController();
            _sqlController.Initialize();
        }

        void OnDestroy()
        {
            _sqlController.Clear();
        }

        
    }
}