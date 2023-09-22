using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TouchMap : UdonSharpBehaviour
    {
        public FlightsPanel flightPanel;
        public DistanceMeasureTool distanceMeasureTool;

        public Slider scaleLevelSlider;

        public float mapScaleRate = 1.1f;

        private RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();

            _Reset();
            _UpdateScaleLevel();
        }

        public void _UpdateScaleLevel()
        {
            transform.localScale = Vector3.one * scaleLevelSlider.value;

            flightPanel.UITextScale = 1f / scaleLevelSlider.value;
            distanceMeasureTool.UITextScale = 1f / scaleLevelSlider.value;
        }

        public void _Reset()
        {
            distanceMeasureTool._FullyReset();

            _rectTransform.anchoredPosition = Vector2.zero;

            transform.localScale = Vector3.one;
            flightPanel.UITextScale = 1f;
            distanceMeasureTool.UITextScale = 1f;
        }
    }
}