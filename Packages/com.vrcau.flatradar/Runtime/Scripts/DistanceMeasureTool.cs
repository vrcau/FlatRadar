using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace FlatRadar
{
    public class DistanceMeasureTool : UdonSharpBehaviour
    {
        private VRCPlayerApi _localPlayer;

        public GameObject measureStartMark;
        public GameObject measureEndMark;

        public LineRenderer lineRenderer;

        public Transform screen;

        public float scaleX = 2f;
        public float scaleY = 2f;

        private ToolStatus _toolStatus = ToolStatus.Disable;

        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            _Reset();
        }

        public void _StartMeasure()
        {
            _Reset();
            _toolStatus = ToolStatus.StartPoint;

            measureStartMark.SetActive(true);
        }

        public void _Reset()
        {
            _toolStatus = ToolStatus.Disable;
            lineRenderer.enabled = false;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);

            measureStartMark.SetActive(false);
            measureEndMark.SetActive(false);
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

                    startPosition = GetMarkPosition(startPosition.z - 0.0001f);
                    measureStartMark.transform.position = startPosition;
                    break;
                case ToolStatus.EndPoint:
                    if (Input.GetMouseButtonDown(0))
                    {
                        _toolStatus = ToolStatus.Disable;
                        return;
                    }

                    measureEndMark.SetActive(true);
                    var endPosition = measureEndMark.transform.position;

                    endPosition = GetMarkPosition(endPosition.z - 0.0001f);
                    measureEndMark.transform.position = endPosition;
                    lineRenderer.SetPosition(1, endPosition);
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

            return new Vector3(
                headTrackingData.position.x - Mathf.Tan(headTrackingData.rotation.y) * distanceToScreen * scaleX,
                headTrackingData.position.y + Mathf.Tan(headTrackingData.rotation.x) * distanceToScreen * scaleY
                , originalZ);
        }
    }

    public enum ToolStatus
    {
        Disable,
        StartPoint,
        EndPoint
    }
}