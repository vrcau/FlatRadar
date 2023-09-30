using FlatRadar.Server;
using UdonSharp;
using UnityEngine.UI;

namespace FlatRadar.Pages
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HomePage : UdonSharpBehaviour
    {
        private FlatRadarTerminal _flatRadarTerminal;
        private FlatRadarServer _flatRadarServer;

        public Text onlineAtcText;
        public Text notamText;

        private void Start()
        {
            _flatRadarTerminal = GetComponentInParent<FlatRadarTerminal>();
            _flatRadarServer = _flatRadarTerminal.flatRadarServer;

            UpdateOnlineAtc();

            notamText.text = _flatRadarServer.notams;
        }

        private void UpdateOnlineAtc()
        {
            var atcCallSigns = _flatRadarServer.atcCallSigns;
            var atcFreq = _flatRadarServer.atcFreq;

            var atcText = "CALLSIGN|   FREQ\n";
            for (var index = 0; index < atcCallSigns.Length; index++)
            {
                var atcCallSign = atcCallSigns[index];
                var freq = atcFreq[index];

                var freqText = freq.PadLeft(7, ' ');
                atcText += $"{atcCallSign}|{freqText}\n";
            }

            onlineAtcText.text = atcText;
        }
    }
}