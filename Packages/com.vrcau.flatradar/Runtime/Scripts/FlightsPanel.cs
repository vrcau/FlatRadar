using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VirtualCNS;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEngine.SceneManagement;
using UdonSharpEditor;
#endif

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlightsPanel : UdonSharpBehaviour
    {
        public GameObject flightIconTemplate;
        public GameObject navaidIconTemplate;
        public Camera terrainCamera;
        public Text flightsText;
        public Transform seaLevel;

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

        [HideInInspector] public Transform[] traffics = { };
        [HideInInspector] public string[] tailNumbers = { };
        [HideInInspector] public string[] callSigns = { };
        [HideInInspector] public GameObject[] ownerDetectors = { };

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
            InitNavaids();
            InitFlightIcons();

            // Terrain
            terrainCamera.orthographicSize = 10000f;
            terrainCamera.enabled = true;
            SendCustomEventDelayedFrames(nameof(_DisableCamera), 1, EventTiming.LateUpdate);
        }

        private void InitFlightIcons()
        {
            _flightIcons = new Transform[traffics.Length];
            _flightTags = new TextMeshProUGUI[traffics.Length];
            _previousPositions = new Vector3[traffics.Length];
            _previousTimes = new float[traffics.Length];

            for (var index = 0; index < traffics.Length; index++)
            {
                var flightPrefab = Instantiate(flightIconTemplate, transform, false);

                flightPrefab.name = callSigns[index];

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

            for (var index = 0; index < traffics.Length; index++)
            {
                // Traffic Data
                var traffic = traffics[index];
                var tailNumber = tailNumbers[index];
                var callSign = callSigns[index];
                var ownerDetector = ownerDetectors[index];

                var position = traffic.position - transform.position;
                var altitude = position.y - seaLevel.position.y * 3.28084f;

                var groundVelocity = Vector3.ProjectOnPlane(position - _previousPositions[index], Vector3.up);
                var groundSpeed = groundVelocity.magnitude / (time - _previousTimes[index]) * 1.94384f;

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
                                 $"{(int)altitude}ft {(int)groundSpeed}kt\n" +
                                 $"{owner}";

                // Update Flights Text
                flightsTextTemp +=
                    $"{callSign,-9}|V20N |{tailNumber,-7}|{(int)altitude,-7}|{((int)groundSpeed),-4}|{owner}\n";
            }

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
            var radarTransform = transform;
            var navaidPosition = sourceTransform.position - radarTransform.position;
            var navaidPositionScale = (Vector3.right * navaidPosition.x + Vector3.up * navaidPosition.z) * mapScale;

            return navaidPositionScale;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Setup()
        {
            var rootObjects = gameObject.scene.GetRootGameObjects();
            var trafficSources = rootObjects.SelectMany(o => o.GetComponentsInChildren<TailNumberManager>()).ToArray();

            traffics = trafficSources.Select(s => s.transform).ToArray();
            tailNumbers = trafficSources.Select(s => s.tailNumber).ToArray();
            callSigns = trafficSources.Select(s => s.callsign).ToArray();
            ownerDetectors = trafficSources.Select(s => s.gameObject).ToArray();
        }
#endif
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(FlightsPanel))]
    public class FlightPanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            base.OnInspectorGUI();

            var panel = target as FlightsPanel;
            if (panel == null) return;

            if (GUILayout.Button("Setup"))
            {
                panel.Setup();
                ;
                EditorUtility.SetDirty(target);
            }
        }
    }

    public class FlightPanelBuildCallback : IVRCSDKBuildRequestedCallback
    {
        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            var flightsPanels = rootObjects.SelectMany(o => o.GetComponentsInChildren<FlightsPanel>()).ToArray();

            foreach (var flightsPanel in flightsPanels)
            {
                flightsPanel.Setup();
            }

            return true;
        }

        public int callbackOrder => 0;
    }
#endif
}