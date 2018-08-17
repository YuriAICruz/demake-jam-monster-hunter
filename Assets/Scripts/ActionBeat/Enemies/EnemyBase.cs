using System;
using ActionBeat.Animation;
using Graphene.BehaviourTree;
using Physics;
using Shooter;
using UnityEngine;
using Behaviour = Graphene.BehaviourTree.Behaviour;

namespace ActionBeat.Enemies
{
    public class EnemyBase : MonoBehaviour
    {
        public enum BlackboardIds
        {
            WalkOnPath = 1,
            StartPath = 2,
            PlayerTarget = 3,
            MoveToTarget = 4,
            IsAngry = 5,
            PlayerIsOnBack = 6,
            AttackCloseDistance = 7,
            AttackBehindDistance = 8,
            AttackFarDistance = 9,
            ReturnToPath = 10,
            Test = 99,
            
        }
        private AnimationController _animationController;
        public Life Life;

        protected Blackboard _blackboard;
        protected Behaviour _tree;

        private void Awake()
        {
            Life.Reset();
            Life.OnDie += Die;
            
            SendMessage("Setup");
        }

        void Start()
        {
            _animationController = new AnimationController(transform, GetComponent<Animator>());
            
            SetupPhysics();
            
            _tree = new Behaviour();
            _blackboard = new Blackboard();
            CreateBehaviour();
        }

        protected virtual void SetupPhysics()
        {
            throw new NotImplementedException();
        }

        protected virtual void CreateBehaviour()
        {
            throw new NotImplementedException();
        }

        protected virtual  void OnTriggered(RaycastHit2D obj)
        {
        }

        protected virtual  void OnCollided(RaycastHit2D obj)
        {
        }

        protected virtual  void Die()
        {
        }
    }
}