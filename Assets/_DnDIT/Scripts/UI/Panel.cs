using UnityEngine;

namespace DnDInitiativeTracker.UI
{
    public abstract class Panel : MonoBehaviour
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