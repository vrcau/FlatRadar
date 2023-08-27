using UdonSharp;
using UnityEngine;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TouchMap : UdonSharpBehaviour
    {
        public override void PostLateUpdate()
        {
            var gm = gameObject;

            var localPosition = gm.transform.localPosition;
            localPosition.z = 0;
            
            gm.transform.localPosition = localPosition; 
            gm.transform.localRotation = Quaternion.identity;
        }
    }
}