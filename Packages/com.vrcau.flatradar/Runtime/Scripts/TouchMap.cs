using System;
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Enums;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TouchMap : UdonSharpBehaviour
    {
        private BoxCollider _collider;
        private Rigidbody _rigidbody;
        public GameObject screen;

        private void Start()
        {
            _collider = GetComponent<BoxCollider>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void PostLateUpdate()
        {
            _rigidbody.velocity = Vector3.zero;
            var gm = gameObject;

            var localPosition = gm.transform.localPosition;
            localPosition.z = 0;
            
            gm.transform.localPosition = localPosition; 
            gm.transform.localRotation = Quaternion.identity;
            
            var toScreenCenter = screen.transform.localPosition - transform.localPosition;
            toScreenCenter.z = 0;
            _collider.center = toScreenCenter;
        }
    }
}