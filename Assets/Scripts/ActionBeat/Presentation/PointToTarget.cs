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
        private Vector2 _size;
        private Vector3 _scale;

        void Setup()
        {
        }

        private void Start()
        {
            _iniPos = transform.position;

            _size = transform.parent.GetComponent<RectTransform>().sizeDelta;
            _scale = transform.parent.localScale;
        }

        private void Update()
        {
            var dir = Target.position - Origin.position;
            var distance = dir.magnitude;

            var angle = Vector2.Angle(Vector2.up, dir.normalized);
            
            transform.eulerAngles = new Vector3(0,0,angle);

            transform.localPosition = dir.normalized * Distance;// * transform.parent.localScale.y;
        }

    }
}