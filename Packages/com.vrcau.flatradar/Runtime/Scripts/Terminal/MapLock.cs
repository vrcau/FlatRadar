using JetBrains.Annotations;
using UdonSharp;
using UnityEngine.UI;

namespace FlatRadar.Terminal
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MapLock : UdonSharpBehaviour
    {
        public ScrollRect scrollRect;
        public Toggle lockToggle;

        private void Start()
        {
            lockToggle.isOn = false;
        }

        [PublicAPI]
        public void _UpdateLock()
        {
            scrollRect.enabled = !lockToggle.isOn;
        }
    }
}