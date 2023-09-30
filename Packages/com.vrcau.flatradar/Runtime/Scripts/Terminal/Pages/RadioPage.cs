using JetBrains.Annotations;
using UdonSharp;
using UnityEngine.UI;
using URC;

namespace FlatRadar.Pages
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RadioPage : UdonSharpBehaviour
    {
        private FlatRadarTerminal _flatRadarTerminal;
        private Transceiver _transceiver;

        public Text currentFrequencyText;
        public Text currentCallSignText;

        private string[] _atcCallSigns = { };
        private string[] _atcFreq = { };

        public Text selectedPresetText;

        private int _selectedPresetIndex;

        private void Start()
        {
            _flatRadarTerminal = GetComponentInParent<FlatRadarTerminal>();

            if (!_flatRadarTerminal)
            {
                enabled = false;
                return;
            }

            _transceiver = _flatRadarTerminal.transceiver;

            if (!_transceiver)
            {
                enabled = false;
                return;
            }

            _atcCallSigns = _flatRadarTerminal.flatRadarServer.atcCallSigns;
            _atcFreq = _flatRadarTerminal.flatRadarServer.atcFreq;

            UpdateSelectedPresetUI();
        }

        private void LateUpdate()
        {
            currentFrequencyText.text = _transceiver.Frequency.ToString("F") + " MHz";
            currentCallSignText.text = _flatRadarTerminal.callSign;
        }

        private void UpdateSelectedPresetUI()
        {
            if (_atcCallSigns.Length == 0)
                selectedPresetText.text = "No Preset Available";

            selectedPresetText.text = $"{_atcCallSigns[_selectedPresetIndex]} - {_atcFreq[_selectedPresetIndex]} MHz";
        }

        [PublicAPI]
        public void _SelectNextPreset()
        {
            if (_atcCallSigns.Length <= 1)
                return;

            if (_selectedPresetIndex + 1 >= _atcCallSigns.Length)
            {
                _selectedPresetIndex = 0;
                UpdateSelectedPresetUI();
                return;
            }

            _selectedPresetIndex++;
            UpdateSelectedPresetUI();
        }

        [PublicAPI]
        public void _SelectPrevPreset()
        {
            if (_atcCallSigns.Length <= 1)
                return;

            if (_selectedPresetIndex - 1 < 0)
            {
                _selectedPresetIndex = _atcCallSigns.Length - 1;
                UpdateSelectedPresetUI();
                return;
            }

            _selectedPresetIndex--;
            UpdateSelectedPresetUI();
        }

        [PublicAPI]
        public void _LoadSelectedPreset()
        {
            if (_atcCallSigns.Length == 0)
                return;

            _transceiver.Frequency = float.Parse(_atcFreq[_selectedPresetIndex]);
            _flatRadarTerminal.callSign = _atcCallSigns[_selectedPresetIndex];
        }
    }
}