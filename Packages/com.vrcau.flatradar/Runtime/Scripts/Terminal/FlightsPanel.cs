using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VirtualCNS;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlightsPanel : UdonSharpBehaviour
    {
        private FlatRadarTerminal _flatRadarTerminal;

        public GameObject flightIconTemplate;
        public GameObject navaidIconTemplate;
        public Camera terrainCamera;
        public Text flightsText;

        public float mapScale = 0.025f;

        [FieldChangeCallback(nameof(UITextScale))]
        private float _uiTextScale = 1f;

        public float UITextScale
        {
            get => _uiTextScale;
            set
            {
                _uiTextScale = value;

                foreach (var navaid in _navaids)
                {
                    navaid.transform.localScale = new Vector3(_uiTextScale, _uiTextScale, 1);
                }

                foreach (var icon in _flightIcons)
                {
                    icon.transform.localScale = new Vector3(_uiTextScale, _uiTextScale, 1);
                }
            }
        }

        private Transform[] _traffics = { };
        private string[] _tailNumbers = { };
        private string[] _callSigns = { };
        private GameObject[] _ownerDetectors = { };

        private Transform _seaLevel;

        private Transform[] _flightIcons = { };
        private TextMeshProUGUI[] _flightTags = { };

        private Transform[] _navaids = { };
        private Text[] _navaidTexts = { };

        private Vector3[] _previousPositions = { };
        private float[] _previousTimes = { };

        private NavaidDatabase _navaidDatabase;

        private const float UpdateInterval = 0.5f;
        private float _lastUpdate;

        private void Start()
        {
            _flatRadarTerminal = GetComponentInParent<FlatRadarTerminal>();

            if (!_flatRadarTerminal)
            {
                enabled = false;
                return;
            }

            _traffics = _flatRadarTerminal.flatRadarServer.traffics;
            _tailNumbers = _flatRadarTerminal.flatRadarServer.tailNumbers;
            _callSigns = _flatRadarTerminal.flatRadarServer.callSigns;
            _ownerDetectors = _flatRadarTerminal.flatRadarServer.ownerDetectors;

            _seaLevel = _flatRadarTerminal.flatRadarServer.seaLevel;

            InitNavaids();
            InitFlightIcons();

            // Terrain
            var cameraGameObject = terrainCamera.gameObject;
            var renderOriginTransform = _flatRadarTerminal.flatRadarServer.renderOrigin.transform;

            cameraGameObject.transform.position = renderOriginTransform.position;
            cameraGameObject.transform.rotation = new Quaternion(0.7f, 0f, 0f, 0.7f);

            terrainCamera.orthographicSize = 10000f;
            terrainCamera.enabled = true;
            SendCustomEventDelayedFrames(nameof(_DisableCamera), 1, EventTiming.LateUpdate);
        }

        private void InitFlightIcons()
        {
            _flightIcons = new Transform[_traffics.Length];
            _flightTags = new TextMeshProUGUI[_traffics.Length];
            _previousPositions = new Vector3[_traffics.Length];
            _previousTimes = new float[_traffics.Length];

            for (var index = 0; index < _traffics.Length; index++)
            {
                var flightPrefab = Instantiate(flightIconTemplate, transform, false);

                flightPrefab.name = _callSigns[index];

                _flightIcons[index] = flightPrefab.transform;
                _flightTags[index] = flightPrefab.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        private void InitNavaids()
        {
            _navaidDatabase = NavaidDatabase.GetInstance();
            if (!_navaidDatabase)
            {
                Debug.LogWarning("NavaidDatabase not found, flat radar won't show navaids");
                return;
            }

            _navaids = new Transform[_navaidDatabase.transforms.Length];
            _navaidTexts = new Text[_navaidDatabase.transforms.Length];

            for (var index = 0; index < _navaidDatabase.transforms.Length; index++)
            {
                var navaidTransform = _navaidDatabase.transforms[index];
                var identify = _navaidDatabase.identities[index];

                var navaid = PlaceIcon(navaidIconTemplate, navaidTransform);

                var navaidText = navaid.GetComponentInChildren<Text>();
                navaidText.text = identify;

                _navaids[index] = navaid.transform;
                _navaidTexts[index] = navaidText;
            }
        }

        public void _DisableCamera()
        {
            terrainCamera.enabled = false;
        }

        private void LateUpdate()
        {
            var time = Time.time;
            if (time - _lastUpdate < UpdateInterval) return;
            _lastUpdate = time;

            var flightsTextTemp = "CALLSIGN |TYPE |REG    |ALT    |GS  |OWNER\n";

            for (var index = 0; index < _traffics.Length; index++)
            {
                // Traffic Data
                var traffic = _traffics[index];
                var tailNumber = _tailNumbers[index];
                var callSign = _callSigns[index];
                var ownerDetector = _ownerDetectors[index];

                var position = traffic.position - transform.position;
                var altitude = position.y - _seaLevel.position.y * 3.28084f;

                var groundVelocity = Vector3.ProjectOnPlane(position - _previousPositions[index], Vector3.up);
                var groundSpeed = groundVelocity.magnitude / (time - _previousTimes[index]) * 1.94384f;

                var verticalSpeed = (position - _previousPositions[index]).y * 60f * 3.28084f;

                _previousPositions[index] = position;
                _previousTimes[index] = time;

                var owner = Networking.GetOwner(ownerDetector).displayName;

                // UI Elements
                var flightIcon = _flightIcons[index];
                var flightTag = _flightTags[index];

                var rotation =
                    Quaternion.AngleAxis(
                        Vector3.SignedAngle(Vector3.forward, Vector3.Scale(groundVelocity, new Vector3(-1, 1, 1)),
                            Vector3.up), Vector3.forward);

                flightIcon.localPosition = ToMapPosition(traffic);
                flightIcon.localRotation = rotation;

                flightTag.transform.localRotation = Quaternion.Inverse(rotation);

                flightTag.text = $"{callSign} {tailNumber}\n" +
                                 $"{(int)altitude}ft {(int)groundSpeed}kt {(int)verticalSpeed}fpm\n" +
                                 $"{owner}";

                // Update Flights Text
                flightsTextTemp +=
                    $"{callSign,-9}|V20N |{tailNumber,-7}|{(int)altitude,-7}|{((int)groundSpeed),-4}|{owner}\n";
            }

            if (flightsText)
                flightsText.text = flightsTextTemp;
        }

        private GameObject PlaceIcon(GameObject go, Transform sourceTransform)
        {
            var icon = Instantiate(go, transform, false);
            icon.transform.localPosition = ToMapPosition(sourceTransform);

            return icon;
        }

        private Vector3 ToMapPosition(Transform sourceTransform)
        {
            var radarTransform = _flatRadarTerminal.flatRadarServer.renderOrigin;
            var navaidPosition = sourceTransform.position - radarTransform.position;
            var navaidPositionScale = (Vector3.right * navaidPosition.x + Vector3.up * navaidPosition.z) * mapScale;

            return navaidPositionScale;
        }
    }
}