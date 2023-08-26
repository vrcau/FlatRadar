﻿using UdonSharp;
using UdonSharpEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VirtualCNS;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlightsPanel : UdonSharpBehaviour
    {
        public GameObject flightIconTemplate;
        public Camera terrainCamera;
        public Text flightsText;
        
        public float mapScale = 0.025f;

        [HideInInspector] public Transform[] traffics = { };
        [HideInInspector] public string[] tailNumbers = { };
        [HideInInspector] public string[] callSigns = { };
        [HideInInspector] public GameObject[] ownerDetectors = { };

        private Transform[] _flightIcons = { };
        private Text[] _flightTags = { };

        private Vector3[] _previousPositions = { };
        private float[] _previousTimes = { };

        private NavaidDatabase _navaidDatabase;

        private const float UpdateInterval = 0.5f;
        private float _lastUpdate;

        private void Start()
        {
            _navaidDatabase = NavaidDatabase.GetInstance();

            if (!_navaidDatabase)
            {
                Debug.LogWarning("NavaidDatabase not found, flat radar won't show navaids");
            }

            _flightIcons = new Transform[traffics.Length];
            _flightTags = new Text[traffics.Length];
            _previousPositions = new Vector3[traffics.Length];
            _previousTimes = new float[traffics.Length];

            for (var index = 0; index < traffics.Length; index++)
            {
                Debug.Log(callSigns[index]);
                var flightPrefab = Instantiate(flightIconTemplate, transform, false);

                flightPrefab.name = callSigns[index];

                _flightIcons[index] = flightPrefab.transform;
                _flightTags[index] = flightPrefab.GetComponentInChildren<Text>();
            }

            terrainCamera.orthographicSize = 10000f;
            terrainCamera.enabled = true;
            SendCustomEventDelayedFrames(nameof(_DisableCamera), 1, EventTiming.LateUpdate);
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

            var scale = mapScale;

            var flightsTextTemp = "CALLSIGN |TYPE |REG    |ALT    |GS  |OWNER\n";

            for (var index = 0; index < traffics.Length; index++)
            {
                // Traffic Data
                var traffic = traffics[index];
                var tailNumber = tailNumbers[index];
                var callSign = callSigns[index];
                var ownerDetector = ownerDetectors[index];

                var position = traffic.position - transform.position;
                var altitude = position.y * 3.28084f;

                var groundVelocity = Vector3.ProjectOnPlane(position - _previousPositions[index], Vector3.up);
                var groundSpeed = groundVelocity.magnitude / (time - _previousTimes[index]) * 1.94384f;

                _previousPositions[index] = position;
                _previousTimes[index] = time;

                var owner = Networking.GetOwner(ownerDetector).displayName;

                flightsTextTemp +=
                    $"{callSign,-9}|V20N |{tailNumber,-7}|{(int)altitude,-7}|{((int)groundSpeed),-4}|{owner}\n";

                // UI Elements
                var flightIcon = _flightIcons[index];
                var flightTag = _flightTags[index];

                var rotation =
                    Quaternion.AngleAxis(
                        Vector3.SignedAngle(Vector3.forward, Vector3.Scale(groundVelocity, new Vector3(-1, 1, 1)),
                            Vector3.up), Vector3.forward);

                flightIcon.localPosition = (Vector3.right * position.x + Vector3.up * position.z) * scale;
                flightIcon.localRotation = rotation;

                flightTag.transform.localRotation = Quaternion.Inverse(rotation);
                flightTag.text = $"{callSign} {tailNumber}\n" +
                                 $"{(int)altitude}ft {(int)groundSpeed}kt\n" +
                                 $"{owner}";
            }

            flightsText.text = flightsTextTemp;
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