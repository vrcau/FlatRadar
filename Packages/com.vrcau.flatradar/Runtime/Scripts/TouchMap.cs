using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.EventSystems;
using VRC.Udon.Common.Enums;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TouchMap : UdonSharpBehaviour
    {
        // private BoxCollider _collider;
        // private Rigidbody _rigidbody;
        // public GameObject screen;

        public FlightsPanel flightPanel;
        public float mapScaleRate = 1.1f;

        private int _scaleLevel = 1;

        public void _MoveMapUp()
        {
            transform.localPosition += Vector3.down * 30 * (mapScaleRate / Mathf.Abs(_scaleLevel));
        }

        public void _MoveMapDown()
        {
            transform.localPosition += Vector3.up * 30 * (mapScaleRate / Mathf.Abs(_scaleLevel));
        }

        public void _MoveMapLeft()
        {
            transform.localPosition += Vector3.right * 30 * (mapScaleRate / Mathf.Abs(_scaleLevel));
        }

        public void _MoveMapRight()
        {
            transform.localPosition += Vector3.left * 30 * (mapScaleRate / Mathf.Abs(_scaleLevel));
        }

        public void _Reset()
        {
            var gameObjectTransform = transform;

            gameObjectTransform.localPosition = Vector3.zero;
            gameObjectTransform.localScale = Vector3.one;
            flightPanel.UITextScale = 1f;
            _scaleLevel = 1;
        }

        public void _ReduceMapScale()
        {
            flightPanel.UITextScale *= mapScaleRate;

            var gameObjectTransform = transform;
            var localScale = gameObjectTransform.localScale;

            localScale = new Vector3(localScale.x / mapScaleRate, localScale.y / mapScaleRate, 1);
            gameObjectTransform.localScale = localScale;

            _scaleLevel--;
        }

        public void _AddMapScale()
        {
            flightPanel.UITextScale /= mapScaleRate;

            var gameObjectTransform = transform;
            var localScale = gameObjectTransform.localScale;

            localScale = new Vector3(localScale.x * mapScaleRate, localScale.y * mapScaleRate, 1);
            gameObjectTransform.localScale = localScale;

            _scaleLevel++;
        }

        // private void Start()
        // {
        //     _collider = GetComponent<BoxCollider>();
        //     _rigidbody = GetComponent<Rigidbody>();
        // }
        //
        // private void LateUpdate()
        // {
        //     _rigidbody.velocity = Vector3.zero;
        //
        //     var gm = gameObject;
        //
        //     var localPosition = gm.transform.localPosition;
        //     localPosition.z = 0;
        //
        //     gm.transform.localPosition = localPosition;
        //     gm.transform.localRotation = Quaternion.identity;
        //
        //     var toScreenCenter = screen.transform.localPosition - transform.localPosition;
        //     toScreenCenter.z = 0;
        //     _collider.center = toScreenCenter;
        // }
    }
}