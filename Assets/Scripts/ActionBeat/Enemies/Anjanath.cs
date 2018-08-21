﻿using System.Collections.Generic;
using System.Linq.Expressions;
using Graphene.BehaviourTree;
using Graphene.BehaviourTree.Actions;
using Graphene.BehaviourTree.Composites;
using Graphene.BehaviourTree.Conditions;
using Graphene.BehaviourTree.Decorators;
using Physics;
using Splines;
using UnityEngine;
using Behaviour = Graphene.BehaviourTree.Behaviour;

namespace ActionBeat.Enemies
{
    public class Anjanath : EnemyBase
    {
        public AnjanathPhysics Physics;

        public Spline Path;
        private Vector3 _iniPos, _lastPos;
        private float _startTime, _lastPathTime;
        private bool _loop;
        private bool _isOnPath = true;

        [Header("Attack Distance")] public float EngageDistance = 5;
        public float AttackFarDistance = 4;
        public float AttackCloseDistance = 2;
        public float AttackBehindDistance = 2;

        [Header("Cooldown Distance")] public float CooldownFarDistance = 1;
        public float CooldownBehindDistance = 1;
        public float CooldownCloseDistance = 1;

        private ZeldaLikeCharacter _player;
        private Vector3 _dir;
        private bool _isDead;

        private event Behaviour.NodeResponseAction Test;

        private void Setup()
        {
            _iniPos = transform.position;

            _loop = Path.GetLoop();
        }

        protected override void SetupPhysics()
        {
            Physics.SetPosition(transform.position);
            Physics.SetCollider(GetComponent<Collider2D>());
            Physics.OnCollisionEnter += OnCollided;
            Physics.OnTriggerEnter += OnTriggered;
        }

        protected override void Die()
        {
            Debug.LogError("Die");
            _isDead = true;
        }

        protected override void CreateBehaviour()
        {
            SetupDelegates();

            _tree.root = new Priority(
                new List<Node>
                {
                    new MemorySequence(new List<Node>() // Engage Player
                    {
                        new CheckDistance(EngageDistance, (int) BlackboardIds.PlayerTarget),
                        new MemoryPriority(
                            new List<Node>
                            {
                                new MemoryPriority(
                                    new List<Node>
                                    {
                                        new MemorySequence(new List<Node>()
                                            {
                                                new CheckBool((int) BlackboardIds.IsAngry),
                                                new CheckDistance(AttackFarDistance, (int) BlackboardIds.PlayerTarget),
                                                new Wait(CooldownFarDistance),
                                                new CallSystemAction((int) BlackboardIds.AttackFarDistance),
                                                new Wait(CooldownFarDistance * 0.6f),
                                            }
                                        ),
                                        new MemorySequence(new List<Node>()
                                            {
                                                new CallSystemActionMemory((int) BlackboardIds.PlayerIsOnBack),
                                                new CheckDistance(AttackBehindDistance, (int) BlackboardIds.PlayerTarget),
                                                new Wait(CooldownBehindDistance),
                                                new CallSystemAction((int) BlackboardIds.AttackBehindDistance),
                                                new Wait(CooldownBehindDistance * 0.6f),
                                            }
                                        ),
                                        new MemorySequence(new List<Node>()
                                            {
                                                new CheckDistance(AttackCloseDistance, (int) BlackboardIds.PlayerTarget),
                                                new Wait(CooldownCloseDistance),
                                                new CallSystemAction((int) BlackboardIds.AttackCloseDistance),
                                                new Wait(CooldownBehindDistance * 0.6f),
                                            }
                                        ),
                                    }
                                ),
                                new CallSystemAction((int) BlackboardIds.MoveToTarget),
                            }
                        ),
                    }),
                    new Sequence(new List<Node>() // Flee
                    {
                        new CheckBool((int) BlackboardIds.False)
                    }),
                    new MemorySequence(new List<Node>() // Roam
                    {
                        new Inverter(new List<Node>()
                        {
                            new CheckDistance(EngageDistance * 1.2f, (int) BlackboardIds.PlayerTarget),
                        }),
                        new CallSystemActionMemory((int) BlackboardIds.ReturnToPath),
                        new CallSystemActionMemory((int) BlackboardIds.WalkOnPath)
                    }),
                }
            );
        }

        private void SetupDelegates()
        {
            _player = FindObjectOfType<ZeldaLikeCharacter>();

            _blackboard.Set((int) BlackboardIds.False, false, _tree.id);
            _blackboard.Set((int) BlackboardIds.True, true, _tree.id);

            _blackboard.Set((int) BlackboardIds.IsAngry, false, _tree.id);

            _blackboard.Set((int) BlackboardIds.PlayerTarget, _player.transform, _tree.id);

            _blackboard.Set((int) BlackboardIds.StartPath, new System.Action(StartPath), _tree.id);
            _blackboard.Set((int) BlackboardIds.MoveToTarget, new System.Action(MoveToTarget), _tree.id);

            _blackboard.Set((int) BlackboardIds.AttackFarDistance, new System.Action(DoAttackFarDistance), _tree.id);
            _blackboard.Set((int) BlackboardIds.AttackBehindDistance, new System.Action(DoAttackBehindDistance), _tree.id);
            _blackboard.Set((int) BlackboardIds.AttackCloseDistance, new System.Action(DoAttackCloseDistance), _tree.id);

            _blackboard.Set((int) BlackboardIds.ReturnToPath, new Behaviour.NodeResponseAction(ReturnToPath), _tree.id);
            _blackboard.Set((int) BlackboardIds.WalkOnPath, new Behaviour.NodeResponseAction(PathUpdate), _tree.id);
            _blackboard.Set((int) BlackboardIds.PlayerIsOnBack, new Behaviour.NodeResponseAction(PlayerIsOnBack), _tree.id);
        }

        private void MoveToTarget()
        {
            _isOnPath = false;
            _lastPathTime = _startTime;

            var dir = _player.transform.position - transform.position;
            Physics.Move(dir.normalized);

            transform.position = Physics.Position;

            LookTo();

            _lastPos = transform.position;
        }

        private void StartPath()
        {
            _startTime = Time.time;
            _lastPos = transform.position;
        }


        private NodeStates ReturnToPath()
        {
            if (_isOnPath)
                return NodeStates.Success;

            var dist = Path.Distance();
            var t = (Time.time - _lastPathTime) / (dist / Physics.Speed);

            var pos = Path.GetPointOnCurve(t);
            pos = _iniPos + new Vector3(pos.x, pos.y);

            var dir = -transform.position + pos;

            Physics.Move(dir.normalized);

            transform.position = Physics.Position;

            if ((pos - transform.position).magnitude < 0.2f)
            {
                _isOnPath = true;
                _startTime = Time.time - (_lastPathTime - _startTime);
            }

            LookTo();

            _lastPos = transform.position;

            return NodeStates.Running;
        }

        private NodeStates PathUpdate()
        {
            if (!_isOnPath)
                return NodeStates.Failure;

            var dist = Path.Distance();
            if ((Time.time - _startTime) / (dist / Physics.Speed) < 1)
            {
                var t = (Time.time - _startTime) / (dist / Physics.Speed);

                var pos = Path.GetPointOnCurve(t);

                transform.position = _iniPos + new Vector3(pos.x, pos.y);

                LookTo();

                Physics.SetPosition(transform.position);
                _lastPos = transform.position;

                return NodeStates.Running;
            }
            else
            {
                StopPath();
                return NodeStates.Success;
            }
        }

        private void LookTo()
        {
            _dir = GetDirection();

            var rotZ = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, rotZ + 180);
        }

        private Vector3 GetDirection()
        {
            var dir = Vector3.Scale((_lastPos - transform.position), new Vector3(1, 1, 0));

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                dir.y = 0;
            }
            else
            {
                dir.x = 0;
            }

            return dir;
        }

        private void StopPath()
        {
            if (_loop)
            {
                StartPath();
            }
        }

        private NodeStates PlayerIsOnBack()
        {
            var playerDir = _player.transform.position - transform.position;

            var angle = Vector2.Angle(_dir, playerDir);

//            Debug.Log("Angle: " + angle);

            if (angle < 15)
                return NodeStates.Success;

            return NodeStates.Failure;
        }


        private void DoAttackCloseDistance()
        {
            Debug.Log("DoAttackCloseDistance");
        }

        private void DoAttackBehindDistance()
        {
            Debug.Log("DoAttackBehindDistance");
        }

        private void DoAttackFarDistance()
        {
            Debug.Log("DoAttackFarDistance");
        }

        private void Update()
        {
            if (_isDead) return;

            _tree.Tick(this.gameObject, _blackboard);
        }
    }
}