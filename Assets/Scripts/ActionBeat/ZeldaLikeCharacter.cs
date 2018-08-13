using System.Collections.Generic;
using ActionBeat.Animation;
using Physics;
using Shooter;
using Shooter.InputManage;
using UnityEngine;

namespace ActionBeat
{
    public class ZeldaLikeCharacter : MonoBehaviour
    {
        public ZeldaLikePhysics Physics;
        private AnimationController _animationController;
        public Life Life;
        public Stamina Stamina;

        private ZeldaLikeInputDispatcher InputDispatcher;

        private bool _isDodging;

        private void Awake()
        {
            Life.Reset();
            Stamina.Reset();
        }

        void Start()
        {
            _animationController = new AnimationController(transform, GetComponent<Animator>());

            Physics.SetPosition(transform.position);
            Physics.SetCollider(GetComponent<Collider2D>());
            Physics.OnCollisionEnter += OnCollided;
            Physics.OnTriggerEnter += OnTriggered;
        }

        private void OnEnable()
        {
            if (InputDispatcher == null)
                InputDispatcher = new ZeldaLikeInputDispatcher(this);

            InputDispatcher.Attack += Attack;
            InputDispatcher.Deffend += Deffend;
            InputDispatcher.Dodge += Dodge;
            InputDispatcher.LeftStick += Move;
        }

        private void OnDisable()
        {
            InputDispatcher.Attack -= Attack;
            InputDispatcher.Deffend -= Deffend;
            InputDispatcher.Dodge -= Dodge;
            InputDispatcher.LeftStick -= Move;
        }

        private void Move(Vector2 dir)
        {
            Physics.Move(dir);
            _animationController.SetVelocity(Physics.Velocity);
            transform.position = Physics.Position;
        }

        private void Deffend()
        {
            Stamina.DoAction(1);
        }

        private void Attack()
        {
            Stamina.DoAction(5);
        }

        private void Dodge()
        {
            if (Physics.Velocity.magnitude <= 0)
                return;

            InputDispatcher.BlockInputs();
            _isDodging = true;
            Physics.Dodge(() => { transform.position = Physics.Position; }, () =>
            {
                InputDispatcher.UnblockInputs();
                _isDodging = false;
            });
            _animationController.Dodge(Physics.Velocity);
        }

        private void OnTriggered(RaycastHit2D obj)
        {
            
        }

        private void OnCollided(RaycastHit2D obj)
        {
            
        }
    }
}