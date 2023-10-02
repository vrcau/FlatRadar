using System;
using JetBrains.Annotations;
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

        public float initialScale = 1f;

        private RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();

            _Reset();
            _UpdateScaleLevel();
        }

        [PublicAPI]
        public void _UpdateScaleLevel()
        {
            transform.localScale = Vector3.one * scaleLevelSlider.value;

            flightPanel.UITextScale = 1f / scaleLevelSlider.value;
            distanceMeasureTool.UITextScale = 1f / scaleLevelSlider.value;
        }

        [PublicAPI]
        public void _Reset()
        {
            distanceMeasureTool._FullyReset();

            _rectTransform.anchoredPosition = Vector2.zero;

            scaleLevelSlider.value = initialScale;

            transform.localScale = Vector3.one * scaleLevelSlider.value;
            flightPanel.UITextScale = 1f / scaleLevelSlider.value;
            distanceMeasureTool.UITextScale = 1f / scaleLevelSlider.value;
        }
    }
}