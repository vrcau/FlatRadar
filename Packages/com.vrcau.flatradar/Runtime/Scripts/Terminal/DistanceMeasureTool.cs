﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DistanceMeasureTool : UdonSharpBehaviour
    {
        private VRCPlayerApi _localPlayer;

        public GameObject measureStartMark;
        public GameObject measureEndMark;
        public Text distanceResultText;

        public LineRenderer lineRenderer;

        public Transform screen;

        public float scaleX = 2f;
        public float scaleY = 2f;

        public float mapScale = 0.025f;

        [FieldChangeCallback(nameof(UITextScale))]
        private float _uiTextScale = 1f;
        public float UITextScale
        {
            get => _uiTextScale;
            set
            {
                _uiTextScale = value;

                var scale = new Vector3(_uiTextScale, _uiTextScale, 1f);
                measureEndMark.transform.localScale = scale;
                measureStartMark.transform.localScale = scale;
            }
        }

        private ToolStatus _toolStatus = ToolStatus.Disable;

        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            _Reset();
        }

        public void _StartMeasure()
        {
            _Reset();
            if (_toolStatus != ToolStatus.Disable)
            {
                _toolStatus = ToolStatus.Disable;
                return;
            }

            _toolStatus = ToolStatus.StartPoint;

            measureStartMark.SetActive(true);
        }

        public void _Reset()
        {
            lineRenderer.enabled = false;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);

            distanceResultText.gameObject.SetActive(false);
            measureStartMark.SetActive(false);
            measureEndMark.SetActive(false);
        }

        public void _FullyReset()
        {
            _toolStatus = ToolStatus.Disable;
            _Reset();
        }

        public override void PostLateUpdate()
        {
            switch (_toolStatus)
            {
                case ToolStatus.StartPoint:
                    if (Input.GetMouseButtonDown(0))
                    {
                        lineRenderer.enabled = true;
                        _toolStatus = ToolStatus.EndPoint;
                        lineRenderer.SetPosition(0, measureStartMark.transform.position);
                        return;
                    }

                    var startPosition = measureStartMark.transform.position;

                    startPosition = GetMarkPosition(startPosition.z);
                    measureStartMark.transform.position = startPosition;
                    break;
                case ToolStatus.EndPoint:
                    if (Input.GetMouseButtonDown(0))
                    {
                        _toolStatus = ToolStatus.Done;
                        distanceResultText.gameObject.SetActive(true);
                        distanceResultText.text = (Vector3.Distance(measureStartMark.transform.localPosition,
                            measureEndMark.transform.localPosition) / mapScale).ToString("F") + "m";
                        return;
                    }

                    measureEndMark.SetActive(true);
                    var endPosition = measureEndMark.transform.position;

                    endPosition = GetMarkPosition(endPosition.z);
                    measureEndMark.transform.position = endPosition;

                    lineRenderer.SetPosition(1, new Vector3(endPosition.x, endPosition.y, endPosition.z - 0.0001f));
                    break;
            }
        }

        private Vector3 GetMarkPosition(float originalZ)
        {
            if (_localPlayer.IsUserInVR())
            {
                var trackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                return new Vector3(trackingData.position.x, trackingData.position.y, originalZ);
            }

            var headTrackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            var distanceToScreen = Mathf.Abs(headTrackingData.position.z - screen.position.z);

            if (headTrackingData.rotation.w >= 0f)
            {
                return new Vector3(
                    headTrackingData.position.x + Mathf.Tan(headTrackingData.rotation.y) * distanceToScreen * scaleX,
                    headTrackingData.position.y - Mathf.Tan(headTrackingData.rotation.x) * distanceToScreen * scaleY
                    , originalZ);
            }
            else
            {
                return new Vector3(
                    headTrackingData.position.x - Mathf.Tan(headTrackingData.rotation.y) * distanceToScreen * scaleX,
                    headTrackingData.position.y + Mathf.Tan(headTrackingData.rotation.x) * distanceToScreen * scaleY
                    , originalZ);
            }
        }
    }

    public enum ToolStatus
    {
        Disable,
        Done,
        StartPoint,
        EndPoint
    }
}