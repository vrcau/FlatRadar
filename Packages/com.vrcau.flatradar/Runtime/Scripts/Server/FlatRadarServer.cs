using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VirtualCNS;
using VRC.Udon.Common.Interfaces;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEngine.SceneManagement;
#endif

namespace FlatRadar.Server
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FlatRadarServer : UdonSharpBehaviour
    {
        [TextArea] public string notams;

        [HideInInspector] public string[] atcCallSigns;
        [HideInInspector] public string[] atcFreq;

        public Transform seaLevel;
        public Transform renderOrigin;

        private FlatRadarTerminal[] _terminals = {};

        [PublicAPI]
        public void _RegisterTerminal(FlatRadarTerminal terminal)
        {
            var newArray = new FlatRadarTerminal[_terminals.Length + 1];
            _terminals.CopyTo(newArray, 0);

            newArray[newArray.Length - 1] = terminal;

            _terminals = newArray;
        }

        [PublicAPI]
        public void _SendEventToTerminals(string eventName, bool isNetworked = false, bool withOutPrefix = false)
        {
            var finalEventName = withOutPrefix ? eventName : isNetworked ? $"FlatRadar_{eventName}" : $"_FlatRadar_{eventName}";

            foreach (var terminal in _terminals)
            {
                if (!isNetworked)
                    terminal.SendCustomEvent(finalEventName);
                else
                    terminal.SendCustomNetworkEvent(NetworkEventTarget.All, finalEventName);
            }
        }

        [HideInInspector] public Transform[] traffics = { };
        [HideInInspector] public string[] tailNumbers = { };
        [HideInInspector] public string[] callSigns = { };
        [HideInInspector] public GameObject[] ownerDetectors = { };

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
    public class FlatRadarServerBuildCallback : IVRCSDKBuildRequestedCallback
    {
        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            var servers = rootObjects.SelectMany(o => o.GetComponentsInChildren<FlatRadarServer>()).ToArray();

            foreach (var server in servers)
            {
                server.Setup();
            }

            return true;
        }

        public int callbackOrder => 0;
    }

    [CustomEditor(typeof(FlatRadarServer))]
    public class FlatRadarServerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            base.OnInspectorGUI();

            var server = target as FlatRadarServer;

            if (!server) return;

            if (GUILayout.Button("Setup"))
            {
                server.Setup();
                EditorUtility.SetDirty(target);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("ATCs", new GUIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white
                }
            });

            for (var index = 0; index < server.atcCallSigns.Length; index++)
            {
                var atcCallSign = server.atcCallSigns[index];
                var atcFreq = server.atcFreq[index];
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var callSign = EditorGUILayout.TextField(atcCallSign);
                        var freq = EditorGUILayout.TextField(atcFreq);
                        if (GUILayout.Button("Remove"))
                        {
                            var newAtcCallSigns = server.atcCallSigns.ToList();
                            var newAtcFreq = server.atcFreq.ToList();

                            newAtcCallSigns.Remove(atcCallSign);
                            newAtcFreq.Remove(atcFreq);

                            server.atcCallSigns = newAtcCallSigns.ToArray();
                            server.atcFreq = newAtcFreq.ToArray();

                            EditorUtility.SetDirty(target);
                            return;
                        }

                        if (!change.changed) continue;

                        server.atcCallSigns[index] = callSign;
                        server.atcFreq[index] = freq;

                        EditorUtility.SetDirty(target);
                    }
                }
            }

            if (GUILayout.Button("Add"))
            {
                var newAtcCallSigns = new string[server.atcCallSigns.Length + 1];
                var newAtcFreq = new string[server.atcFreq.Length + 1];

                server.atcCallSigns.CopyTo(newAtcCallSigns, 0);
                server.atcFreq.CopyTo(newAtcFreq, 0);

                newAtcCallSigns[newAtcCallSigns.Length - 1] = "ATC CALLSIGN";
                newAtcFreq[newAtcFreq.Length - 1] = "ATC FREQ";

                server.atcCallSigns = newAtcCallSigns;
                server.atcFreq = newAtcFreq;

                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}