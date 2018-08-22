using UiGenerics;
using UnityEngine;

namespace ActionBeat.Presentation
{
    public class PointToTarget : ImageView
    {
        public Transform Target;
        public Transform Origin;
        private Vector3 _iniPos;
        public float Distance;

        void Setup()
        {
            _iniPos = transform.position;
        }

        private void Update()
        {
            var dir = Target.position - Origin.position;
            var distance = dir.magnitude;

            var angle = Vector2.Angle(Vector2.up, dir.normalized);
            
            transform.eulerAngles = new Vector3(0,0,angle);

            transform.position = _iniPos + dir.normalized * Distance;
        }

    }
}