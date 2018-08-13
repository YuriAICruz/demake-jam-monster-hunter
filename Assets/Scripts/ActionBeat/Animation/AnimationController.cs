using System;
using UnityEngine;

namespace ActionBeat.Animation
{
    public class AnimationController
    {
        private readonly Transform _transform;
        private readonly Animator _animator;
        private Vector3 _lastPos;
        private int _lastDir;

        public AnimationController(Transform transform, Animator animator)
        {
            _transform = transform;
            _animator = animator;

            _lastPos = _transform.position;
        }

        int GetDir(Vector2 velocity)
        {
            var dir = 0;
            if (velocity.magnitude == 0)
            {
                return _lastDir;
            }

            if (velocity.x > velocity.y)
            {
                if (velocity.x > 0)
                    dir = 2;
                else
                    dir = 4;
            }
            else
            {
                if (velocity.y > 0)
                    dir = 1;
                else
                    dir = 3;
            }
            _lastDir = dir;
            return dir;
        }

        public void SetVelocity(Vector2 velocity)
        {
            velocity *= 100;

            var dir = GetDir(velocity);
            
            _animator.SetFloat("Speed", velocity.magnitude);

            if (velocity.magnitude == 0)
            {
                switch (dir)
                {
                    case 1:
                        velocity = new Vector2(0, 1);
                        break;
                    case 2:
                        velocity = new Vector2(1, 0);
                        break;
                    case 3:
                        velocity = new Vector2(-1, 0);
                        break;
                    case 4:
                        velocity = new Vector2(0, -1);
                        break;
                }
            }

            if (velocity.x < 0)
                _transform.localScale = new Vector3(-1, 1, 1);
            else
                _transform.localScale = new Vector3(1, 1, 1);

            _animator.SetFloat("X", velocity.x);
            _animator.SetFloat("Y", velocity.y);
        }

        public void Dodge(Vector2 direction)
        {
            _animator.SetTrigger("Dodge");
        }
    }
}