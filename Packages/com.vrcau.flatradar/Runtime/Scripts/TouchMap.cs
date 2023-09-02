using UdonSharp;
using UnityEngine;

namespace FlatRadar
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TouchMap : UdonSharpBehaviour
    {
        public FlightsPanel flightPanel;
        public DistanceMeasureTool distanceMeasureTool;
        public float mapScaleRate = 1.1f;

        private int _scaleLevel = 1;

        public void _MoveMapUp()
        {
            distanceMeasureTool._FullyReset();
            transform.localPosition += Vector3.down * 30 * (mapScaleRate / Mathf.Abs(_scaleLevel));
        }

        public void _MoveMapDown()
        {
            distanceMeasureTool._FullyReset();
            transform.localPosition += Vector3.up * 30 * (mapScaleRate / Mathf.Abs(_scaleLevel));
        }

        public void _MoveMapLeft()
        {
            distanceMeasureTool._FullyReset();
            transform.localPosition += Vector3.right * 30 * (mapScaleRate / Mathf.Abs(_scaleLevel));
        }

        public void _MoveMapRight()
        {
            distanceMeasureTool._FullyReset();
            transform.localPosition += Vector3.left * 30 * (mapScaleRate / Mathf.Abs(_scaleLevel));
        }

        public void _Reset()
        {
            distanceMeasureTool._FullyReset();
            var gameObjectTransform = transform;

            gameObjectTransform.localPosition = Vector3.zero;
            gameObjectTransform.localScale = Vector3.one;
            flightPanel.UITextScale = 1f;
            _scaleLevel = 1;
        }

        public void _ReduceMapScale()
        {
            distanceMeasureTool._FullyReset();
            flightPanel.UITextScale *= mapScaleRate;

            var gameObjectTransform = transform;
            var localScale = gameObjectTransform.localScale;

            localScale = new Vector3(localScale.x / mapScaleRate, localScale.y / mapScaleRate, 1);
            gameObjectTransform.localScale = localScale;

            _scaleLevel--;
        }

        public void _AddMapScale()
        {
            distanceMeasureTool._FullyReset();
            flightPanel.UITextScale /= mapScaleRate;

            var gameObjectTransform = transform;
            var localScale = gameObjectTransform.localScale;

            localScale = new Vector3(localScale.x * mapScaleRate, localScale.y * mapScaleRate, 1);
            gameObjectTransform.localScale = localScale;

            _scaleLevel++;
        }
    }
}