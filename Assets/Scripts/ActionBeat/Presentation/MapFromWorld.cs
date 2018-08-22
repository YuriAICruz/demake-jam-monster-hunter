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
            var pos = _limits.LeftBotton - Target.position Vector3.Dot( (_limits.RightTop - _limits.LeftBotton);
            pos.z = 0;
            
            pos *= new Vector2(475,300);
            
            
            transform.localPosition = pos;
        }
    }
}