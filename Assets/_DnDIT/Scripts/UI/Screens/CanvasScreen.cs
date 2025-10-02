using UnityEngine;

namespace DnDInitiativeTracker.UI
{
    public abstract class CanvasScreen : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public abstract void Initialize();
    }
}