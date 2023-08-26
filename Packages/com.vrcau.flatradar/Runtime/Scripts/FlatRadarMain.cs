using System;
using System.Linq;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using URC;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlatRadarMain : UdonSharpBehaviour
    {
        public string callSign;
        [TextArea] public string notams;

        public Transceiver transceiver;

        [Header("UI Elements")] public Text statusText;
        public Text metarText;
        public Text onlineAtcText;
        public Text notamText;

        [HideInInspector] public string[] atcCallSigns;
        [HideInInspector] public string[] atcFreq;

        private const float UpdateInterval = 1f;
        private float _lastUpdate;

        private void Start()
        {
            statusText.text = "";
            metarText.text = "METAR INOP";

            notamText.text = notams;

            UpdateOnlineAtc();
        }

        private void LateUpdate()
        {
            if (Time.time - _lastUpdate < UpdateInterval) return;
            _lastUpdate = Time.time;

            UpdateStatusText();
        }

        private void UpdateOnlineAtc()
        {
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

        private void UpdateStatusText()
        {
            statusText.text = "";
            if (transceiver)
            {
                var transmit = transceiver._GetTransmit();
                var receive = transceiver.receiver.Active;
                statusText.text =
                    $"{transceiver.Frequency:F} {(transmit ? "<color=#43cf7c>RX</color>" : "RX")} {(receive ? "<color=#43cf7c>TX</color>" : "TX")} | ";
            }
            else
            {
                statusText.text = "RADIO INOP | ";
            }

            var zuluTime = DateTimeOffset.UtcNow;
            var localTime = DateTimeOffset.Now;
            statusText.text += $"{callSign} | {zuluTime:T}z | UTC+{localTime.Offset.Hours} {localTime:T}";
        }
        
        
        public void _TogglePower()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(FlatRadarMain))]
    public class FlatRadarMainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            base.OnInspectorGUI();

            var radar = target as FlatRadarMain;

            if (radar == null) return;

            GUILayout.FlexibleSpace();
            GUILayout.Label("ATCs", new GUIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white
                }
            });

            for (var index = 0; index < radar.atcCallSigns.Length; index++)
            {
                var atcCallSign = radar.atcCallSigns[index];
                var atcFreq = radar.atcFreq[index];
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var callSign = EditorGUILayout.TextField(atcCallSign);
                        var freq = EditorGUILayout.TextField(atcFreq);
                        if (GUILayout.Button("Remove"))
                        {
                            var newAtcCallSigns = radar.atcCallSigns.ToList();
                            var newAtcFreq = radar.atcFreq.ToList();

                            newAtcCallSigns.Remove(atcCallSign);
                            newAtcFreq.Remove(atcFreq);

                            radar.atcCallSigns = newAtcCallSigns.ToArray();
                            radar.atcFreq = newAtcFreq.ToArray();

                            EditorUtility.SetDirty(target);
                            return;
                        }

                        if (!change.changed) continue;

                        radar.atcCallSigns[index] = callSign;
                        radar.atcFreq[index] = freq;
                    }
                }
            }

            if (GUILayout.Button("Add"))
            {
                var newAtcCallSigns = new string[radar.atcCallSigns.Length + 1];
                var newAtcFreq = new string[radar.atcFreq.Length + 1];

                radar.atcCallSigns.CopyTo(newAtcCallSigns, 0);
                radar.atcFreq.CopyTo(newAtcFreq, 0);

                newAtcCallSigns[newAtcCallSigns.Length - 1] = "ATC CALLSIGN";
                newAtcFreq[newAtcFreq.Length - 1] = "ATC FREQ";

                radar.atcCallSigns = newAtcCallSigns;
                radar.atcFreq = newAtcFreq;

                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}