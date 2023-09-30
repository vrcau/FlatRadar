using System;
using FlatRadar.Server;
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
    public class FlatRadarTerminal : UdonSharpBehaviour
    {
        public FlatRadarServer flatRadarServer;

        public string callSign;

        public Transceiver transceiver;

        [Header("UI Elements")]
        public Text statusText;
        public Text metarText;

        private void Start()
        {
            statusText.text = "";
            metarText.text = "METAR INOP";
        }

        private void LateUpdate()
        {
            UpdateStatusText();
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
    [CustomEditor(typeof(FlatRadarTerminal))]
    public class FlatRadarMainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            base.OnInspectorGUI();

            var radar = target as FlatRadarTerminal;

            if (!radar) return;

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
        }
    }
#endif
}