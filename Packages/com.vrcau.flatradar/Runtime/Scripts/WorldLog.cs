using InariUdon.UI;
using TMPro;
using UdonSharp;
using UnityEngine;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WorldLog : UdonSharpBehaviour
    {
        public TextMeshProUGUI logTextOutput;

        private UdonLogger _logger;

        private const float UpdateInterval = 1f;
        private float _lastUpdate;

        private void Start()
        {
            logTextOutput.text = "";
            _logger = UdonLogger.GetInstance();
        }

        private void LateUpdate()
        {
            if (Time.time - _lastUpdate < UpdateInterval) return;
            _lastUpdate = Time.time;
            
            logTextOutput.text = _logger.text.text;
        }
    }
}
