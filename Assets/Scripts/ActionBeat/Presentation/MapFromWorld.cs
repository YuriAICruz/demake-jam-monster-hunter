using TMPro;
using UiGenerics;
using UnityEngine;

namespace ActionBeat.Presentation
{
    public class MapFromWorld : ImageView
    {
        public Transform Target;

        private LevelLimits _limits;
        
        void Setup(){}

        private void Start()
        {
            _limits = FindObjectOfType<LevelLimits>();
        }

        private void Update()
        {
            SetPosition();
        }

        private void SetPosition()
        {
            var pos = (-_limits.LeftBotton + Target.position);
            var size = (_limits.RightTop - _limits.LeftBotton);
            var relativePos = new Vector2(pos.x/size.x, pos.y/size.y);

            var parentSize = transform.parent.GetComponent<RectTransform>().sizeDelta;
            relativePos = relativePos * parentSize - parentSize/2;
            
            
            transform.localPosition = relativePos;
        }
    }
}