using JetBrains.Annotations;
using UdonSharp;
using UnityEngine.UI;
using VRC.SDKBase;

namespace FlatRadar.Terminal
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MapLock : UdonSharpBehaviour
    {
        public ScrollRect scrollRect;
        public Toggle lockToggle;

        private void Start()
        {
            lockToggle.isOn = Networking.LocalPlayer.IsUserInVR();

            _UpdateLock();
        }

        [PublicAPI]
        public void _UpdateLock()
        {
            scrollRect.enabled = !lockToggle.isOn;
        }
    }
}