using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TouchMap : UdonSharpBehaviour
    {
        private void LateUpdate()
        {
            var gm = gameObject;

            var localPosition = gm.transform.localPosition;
            localPosition.z = 0;
            
            gm.transform.localPosition = localPosition; 
            gm.transform.localRotation = Quaternion.identity;
        }
    }
}