using System.Collections;
using System.Collections.Generic;
using System.IO;
using Graphene.BehaviourTree;
using Graphene.BehaviourTree.Actions;
using Graphene.BehaviourTree.Composites;
using Graphene.BehaviourTree.Conditions;
using Graphene.BehaviourTree.Decorators;
using Physics;
using Shooter;
using Splines;
using UnityEngine;
using Utils;
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

        [Header("FarDistance")] public float AttackFarDistance = 4;
        public float FarCooldown = 1;
        public AttackAtributes FarAtributes;

        [Header("CloseDistance")] public float AttackCloseDistance = 2;
        public float CloseCooldown = 1;
        public AttackAtributes CloseAtributes;

        [Header("BehindDistance")] public float AttackBehindDistance = 2;
        public float BehindCooldown = 1;
        public AttackAtributes BehindAtributes;

        private ZeldaLikeCharacter _player;
        private Vector3 _lastPlayerPos;
        private Vector3 _dir;
        private bool _isDead;
        private int _mask;

        private event Behaviour.NodeResponseAction Test;

        private void Setup()
        {
            _iniPos = transform.position;

            _loop = Path.GetLoop();

            _mask = Physics2D.GetLayerCollisionMask(gameObject.layer);

            _mask |= (1 << LayerMask.NameToLayer("Player"));
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
            Invoke("EndGame", 2);
            _isDead = true;
        }

        void EndGame()
        {
            _manager.EndGame();
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
                                        new MemorySequence(new List<Node>() // FarDistance
                                            {
                                                new CheckBool((int) BlackboardIds.IsAngry),
                                                new CheckDistance(AttackFarDistance, (int) BlackboardIds.PlayerTarget),
                                                new Wait(FarCooldown),
                                                new CallSystemAction((int) BlackboardIds.AttackFarDistance),
                                                new Wait(FarAtributes.Duration),
                                            }
                                        ),
                                        new MemorySequence(new List<Node>() // BehindDistance
                                            {
                                                new CallSystemActionMemory((int) BlackboardIds.PlayerIsOnBack),
                                                new CheckDistance(AttackBehindDistance, (int) BlackboardIds.PlayerTarget),
                                                new Wait(BehindCooldown),
                                                new CallSystemAction((int) BlackboardIds.AttackBehindDistance),
                                                new Wait(BehindAtributes.Duration),
                                            }
                                        ),
                                        new MemorySequence(new List<Node>() // CloseDistance
                                            {
                                                new CheckDistance(AttackCloseDistance, (int) BlackboardIds.PlayerTarget),
                                                new Wait(CloseCooldown),
                                                new CallSystemAction((int) BlackboardIds.AttackCloseDistance),
                                                new Wait(CloseAtributes.Duration),
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
            _lastPlayerPos = _player.transform.position;
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


        private bool CheckAndDoDamage(int damage, RaycastHit2D hit)
        {
            if (hit.collider != null)
            {
                var dmg = hit.collider.GetComponent<IDamageble>();
                if (dmg != null)
                {
                    dmg.DoDamage(damage);
                    return true;
                }
            }
            return false;
        }


        private void DoAttackCloseDistance()
        {
            StartCoroutine(DoAttackCloseDistanceRoutine());
        }

        IEnumerator DoAttackCloseDistanceRoutine()
        {
            var dir = (_lastPlayerPos - transform.position).normalized;
            var time = 0f;

            RaycastHit2D hit;

            while (time <= CloseAtributes.Duration)
            {
                Physics.Move(dir * (CloseAtributes.Distance / CloseAtributes.Duration));
                transform.position = Physics.Position;

                hit = Physics2D.Raycast(transform.position, dir, CloseAtributes.Distance * (time / CloseAtributes.Duration), _mask);
                CheckAndDoDamage(CloseAtributes.Damage, hit);

                yield return new WaitForChangedResult();
                time += Time.deltaTime;
            }

            hit = Physics2D.Raycast(transform.position, dir, CloseAtributes.Distance, _mask);
            CheckAndDoDamage(CloseAtributes.Damage, hit);
        }

        private void DoAttackBehindDistance()
        {
            StartCoroutine(DoAttackBehindDistanceRoutine());
        }

        IEnumerator DoAttackBehindDistanceRoutine()
        {
            var dir = _dir.normalized;
            var time = 0f;
            var dur = BehindAtributes.Duration / 2 - BehindCooldown / 4;

            RaycastHit2D hit;
            while (time <= dur)
            {
                var sin = Mathf.Sin((time / dur) * Mathf.PI);
                var modir = new Vector2(dir.x, dir.y).Rotate(60 + sin * 20 - 10f);

                Debug.DrawRay(transform.position, modir * BehindAtributes.Distance, Color.magenta, 1);
                hit = Physics2D.Raycast(transform.position, modir, BehindAtributes.Distance, _mask);
                CheckAndDoDamage(BehindAtributes.Damage, hit);

                yield return new WaitForChangedResult();
                time += Time.deltaTime;
            }

            yield return new WaitForSeconds(BehindCooldown / 2);
            time = 0f;

            while (time <= dur)
            {
                var sin = Mathf.Sin((time / dur) * Mathf.PI);
                var modir = new Vector2(dir.x, dir.y).Rotate(sin * 15 - 7.5f);

                Debug.DrawRay(transform.position, modir * BehindAtributes.Distance, Color.magenta, 1);
                hit = Physics2D.Raycast(transform.position, modir, BehindAtributes.Distance, _mask);
                CheckAndDoDamage(BehindAtributes.Damage, hit);

                yield return new WaitForChangedResult();
                time += Time.deltaTime;
            }
            
            var todir = -transform.position + _lastPlayerPos;

            Physics.Move(dir.normalized);

            transform.position = Physics.Position;

            LookTo();

            _lastPos = transform.position;
        }

        private void DoAttackFarDistance()
        {
            StartCoroutine(DoAttackFarDistanceRoutine());
        }

        IEnumerator DoAttackFarDistanceRoutine()
        {
            var dir = (_player.transform.position - transform.position).normalized;
            _lastPlayerPos = _player.transform.position;
            
            LookTo();

            _lastPos = transform.position;
            
            var time = 0f;
            RaycastHit2D hit;
            
            while (time <= FarAtributes.Duration)
            {
                Physics.Move(dir * (FarAtributes.Distance / FarAtributes.Duration));
                transform.position = Physics.Position;

                yield return new WaitForChangedResult();
                time += Time.deltaTime;
            }
            
            hit = Physics2D.Raycast(transform.position, dir, FarAtributes.Distance, _mask);
            CheckAndDoDamage(FarAtributes.Damage, hit);
            
            _blackboard.Set((int) BlackboardIds.IsAngry, false, _tree.id);
            
            yield return new WaitForChangedResult();
        }

        private void Update()
        {
            if (_isDead) return;

            _tree.Tick(this.gameObject, _blackboard);
        }

        public override void DoDamage(int damage)
        {
            base.DoDamage(damage);

            if (Life.Hp / (float)Life.MaxHp < 0.6f)
            {
                _blackboard.Set((int) BlackboardIds.IsAngry, true, _tree.id);
                Hoar();
            }
        }

        private void Hoar()
        {
            Debug.Log("Hoar");
        }
    }
}