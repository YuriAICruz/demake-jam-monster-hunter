using UiGenerics;
using UnityEngine;

namespace ActionBeat.Presentation
{
    public class DamageInfo : TextView
    {
        private int _damage;
        private bool _canAnimate;
        public float Speed;
        public float Duration = 0.6f;
        private float _startTime;
        private Transform _canvas;

        void Setup()
        {
            _canvas = GameObject.Find("WorldCanvas").transform;
        }

        public void SetDamage(int damage, Vector3 position)
        {
            transform.SetParent(_canvas);
            transform.localScale = Vector3.one;
            _damage = damage;
            Text.text = _damage.ToString();
            transform.position = position;
            _startTime = Time.time;
            _canAnimate = true;
        }

        private void Update()
        {
            if (!_canAnimate) return;

            transform.Translate(Vector2.up * Time.deltaTime * Speed);
            
            if(Time.time > _startTime + Duration)
                Destroy(gameObject);
        }
    }
}