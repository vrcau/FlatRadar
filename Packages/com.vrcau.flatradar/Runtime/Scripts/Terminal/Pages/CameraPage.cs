using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace FlatRadar.Pages
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CameraPage : UdonSharpBehaviour
    {
        private FlatRadarTerminal _flatRadarTerminal;

        private string[] _cameraNames = { };
        private Transform[] _cameraPositions = { };

        private int _currentCameraIndex;

        public Text currentCameraNameText;
        public Camera airportCamera;
        public GameObject cameraView;

        private void Start()
        {
            _flatRadarTerminal = GetComponentInParent<FlatRadarTerminal>();

            _cameraNames = _flatRadarTerminal.flatRadarServer.cameraNames;
            _cameraPositions = _flatRadarTerminal.flatRadarServer.cameraPositions;

            if (_cameraNames.Length == 0 || _cameraPositions.Length == 0)
            {
                cameraView.SetActive(false);
                gameObject.SetActive(false);
                return;
            }

            UpdateCamera();
        }

        [PublicAPI]
        public void _NextCamera()
        {
            _currentCameraIndex++;
            UpdateCamera();
        }

        [PublicAPI]
        public void _PrevCamera()
        {
            if (_currentCameraIndex - 1 < 0)
            {
                _currentCameraIndex = _cameraNames.Length - 1;
            }
            else
            {
                _currentCameraIndex--;
            }

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (_cameraNames.Length == 0 || _cameraPositions.Length == 0)
                return;

            if (_currentCameraIndex < 0 || _currentCameraIndex >= _cameraNames.Length ||
                _currentCameraIndex >= _cameraPositions.Length)
            {
                _currentCameraIndex = 0;
            }

            var newCameraPosition = _cameraPositions[_currentCameraIndex];
            var cameraPosition = airportCamera.transform;

            cameraPosition.position = newCameraPosition.position;
            cameraPosition.rotation = newCameraPosition.rotation;

            currentCameraNameText.text = _cameraNames[_currentCameraIndex];
        }
    }
}