using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using ActionBeat.Animation;
using Debuging;
using Physics;
using Shooter;
using Shooter.InputManage;
using UnityEngine;
using Utils;

namespace ActionBeat
{
    [Serializable]
    public struct AttackAtributes
    {
        public int Damage;
        public float Distance, Duration;
        public bool IsRight;
        public int StaminaCost;
    }

    public class ZeldaLikeCharacter : MonoBehaviour, IDamageble
    {
        [Space] public ZeldaLikePhysics Physics;
        private AnimationController _animationController;

        public Action OnOpenMap, OnCloseMap;


        [Space] public Life Life;
        [SerializeField] private float _invencibilityDuration;
        private float _lastHit;

        [Space] public Stamina Stamina;

        private ZeldaLikeInputDispatcher _inputDispatcher;

        private bool _isDodging;
        private bool _isJumping;
        private bool _canInteract;
        private bool _mapOpen;
        private Collider2D _collider;
        private int _mask;

        [Header("Attributes")] public AttackAtributes OverheadSlashAttrib;
        public AttackAtributes RisingSlashAttrib;
        public AttackAtributes ChargedSlashAttrib;
        public AttackAtributes WideSlashAttrib;
        public AttackAtributes TrueChargedSlashComboAttrib;
        public AttackAtributes TrueChargedSlashComboFinalAttrib;
        public AttackAtributes FowardLungingAttackComboAttrib;
        public AttackAtributes FowardLungingAttackComboFinalAttrib;
        public AttackAtributes StationaryComboAttrib;
        public AttackAtributes StationaryComboFinalAttrib;
        private IInteractible _interactible;


        private void Awake()
        {
            Life.Reset();
            Life.OnDie += Die;

            Stamina.Reset();
            Stamina.Stun += Stun;
        }

        private void Start()
        {
            _mask = Physics2D.GetLayerCollisionMask(gameObject.layer);

            _mask |= (1 << LayerMask.NameToLayer("Enemy"));

            _animationController = new AnimationController(transform, GetComponent<Animator>());

            Physics.SetPosition(transform.position);
            Physics.SetCollider(GetComponent<Collider2D>());
            Physics.OnCollisionEnter += OnCollided;
            Physics.OnTriggerEnter += OnTriggered;
            Physics.OnJump += Jump;
        }

        private void OnEnable()
        {
            if (_inputDispatcher == null)
                _inputDispatcher = new ZeldaLikeInputDispatcher(this);

            _inputDispatcher.OverheadSlash += OverheadSlash;
            _inputDispatcher.RisingSlash += RisingSlash;
            _inputDispatcher.ChargedSlash += ChargedSlash;
            _inputDispatcher.WideSlash += WideSlash;
            _inputDispatcher.TrueChargedSlashCombo += TrueChargedSlashCombo;
            _inputDispatcher.TrueChargedSlashComboFinal += TrueChargedSlashComboFinal;
            _inputDispatcher.FowardLungingAttackCombo += FowardLungingAttackCombo;
            _inputDispatcher.FowardLungingAttackComboFinal += FowardLungingAttackComboFinal;
            _inputDispatcher.StationaryCombo += StationaryCombo;
            _inputDispatcher.StationaryComboFinal += StationaryComboFinal;
            _inputDispatcher.Deffend += Deffend;
            _inputDispatcher.Dodge += Dodge;
            _inputDispatcher.LeftStick += Move;
            _inputDispatcher.Interact += Interact;
            _inputDispatcher.MapToggle += MapToggle;
        }

        private void OnDisable()
        {
            _inputDispatcher.OverheadSlash -= OverheadSlash;
            _inputDispatcher.RisingSlash -= RisingSlash;
            _inputDispatcher.ChargedSlash -= ChargedSlash;
            _inputDispatcher.WideSlash -= WideSlash;
            _inputDispatcher.TrueChargedSlashCombo -= TrueChargedSlashCombo;
            _inputDispatcher.FowardLungingAttackCombo -= FowardLungingAttackCombo;
            _inputDispatcher.StationaryCombo -= StationaryCombo;
            _inputDispatcher.Deffend -= Deffend;
            _inputDispatcher.Dodge -= Dodge;
            _inputDispatcher.LeftStick -= Move;
            _inputDispatcher.Interact -= Interact;
            _inputDispatcher.MapToggle -= MapToggle;
        }

        private void Move(Vector2 dir)
        {
            Physics.Move(dir);
            _animationController.SetVelocity(Physics.Velocity);
            transform.position = Physics.Position;
        }

        void DoAttack(AttackAtributes attrib)
        {
            StartCoroutine(Attack(attrib.Damage, attrib.Distance, attrib.Duration, attrib.IsRight));
        }

        IEnumerator Attack(int damage, float size, float duration, bool isRight)
        {
            var dir = (_animationController.Direction.normalized * size).Rotate(isRight ? 20 : -20);
            Debug.DrawRay(transform.position, dir, Color.red, 0.6f);

            var hit = Physics2D.Raycast(transform.position, dir, size, _mask);

            var hited = CheckAndDoDamage(damage, hit);

            yield return new WaitForSeconds(duration / 2);

            dir = (_animationController.Direction.normalized * size);
            Debug.DrawRay(transform.position, dir, Color.red, 0.6f);

            if (!hited)
            {
                hit = Physics2D.Raycast(transform.position, dir, size, _mask);
                hited = CheckAndDoDamage(damage, hit);
            }

            yield return new WaitForSeconds(duration / 2);

            dir = (_animationController.Direction.normalized * size).Rotate(isRight ? -20 : 20);
            Debug.DrawRay(transform.position, dir, Color.red, 0.6f);

            if (!hited)
            {
                hit = Physics2D.Raycast(transform.position, dir, size, _mask);
                hited = CheckAndDoDamage(damage, hit);
            }
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


        private void WideSlash()
        {
            if(_canInteract) return;
            
            if (!CanAttack()) return;

            if (!Stamina.DoAction(WideSlashAttrib.StaminaCost)) return;
            
            DoAttack(WideSlashAttrib);

            _animationController.WideSlash();
        }

        private void StationaryCombo()
        {
            if (!CanAttack()) return;

            if (!Stamina.DoAction(StationaryComboAttrib.StaminaCost)) return;

            DoAttack(StationaryComboAttrib);

            _animationController.StationaryCombo();
        }

        private void FowardLungingAttackCombo()
        {
            if (!CanAttack()) return;

            if (!Stamina.DoAction(FowardLungingAttackComboAttrib.StaminaCost)) return;

            DoAttack(FowardLungingAttackComboAttrib);

            _animationController.FowardLungingAttackCombo();
        }

        private void TrueChargedSlashCombo()
        {
            if (!CanAttack()) return;

            if (!Stamina.DoAction(TrueChargedSlashComboAttrib.StaminaCost)) return;

            DoAttack(TrueChargedSlashComboAttrib);

            _animationController.TrueChargedSlashCombo();
        }

        private void ChargedSlash()
        {
            if (!CanAttack()) return;

            if (!Stamina.DoAction(ChargedSlashAttrib.StaminaCost)) return;

            DoAttack(ChargedSlashAttrib);

            _animationController.ChargedSlash();
        }

        private void RisingSlash()
        {
            if (!CanAttack()) return;

            if (!Stamina.DoAction(RisingSlashAttrib.StaminaCost)) return;

            DoAttack(RisingSlashAttrib);

            _animationController.RisingSlash();
        }

        private void OverheadSlash()
        {
            if (!CanAttack()) return;

            if (!Stamina.DoAction(OverheadSlashAttrib.StaminaCost)) return;

            DoAttack(OverheadSlashAttrib);

            _animationController.OverheadSlash();
        }

        private void StationaryComboFinal()
        {
            if (!CanAttack()) return;

            if (!Stamina.DoAction(StationaryComboFinalAttrib.StaminaCost)) return;

            DoAttack(StationaryComboFinalAttrib);

            _animationController.StationaryComboFinal();
        }

        private void FowardLungingAttackComboFinal()
        {
            if (!CanAttack()) return;

            if (!Stamina.DoAction(FowardLungingAttackComboFinalAttrib.StaminaCost)) return;

            DoAttack(FowardLungingAttackComboFinalAttrib);

            _animationController.FowardLungingAttackComboFinal();
        }

        private void TrueChargedSlashComboFinal()
        {
            if (!CanAttack()) return;

            if (!Stamina.DoAction(TrueChargedSlashComboFinalAttrib.StaminaCost)) return;

            DoAttack(TrueChargedSlashComboFinalAttrib);

            _animationController.TrueChargedSlashComboFinal();
        }

        private void Deffend(bool isOn)
        {
            if (!CanDeffend()) return;
            _animationController.Deffend(isOn);
        }


        private void OnTriggered(RaycastHit2D hit)
        {
            _interactible = hit.transform.GetComponent<IInteractible>();
            if(_interactible == null) return;

            _canInteract = true;
        }

        private void OnCollided(RaycastHit2D obj)
        {
        }
        

        private void Dodge()
        {
            if (_inputDispatcher.IsDeffending || Physics.Velocity.magnitude <= 0)
                return;

            Stamina.DoAction(5);

            _inputDispatcher.BlockInputs();
            _isDodging = true;
            Physics.Dodge(() => { transform.position = Physics.Position; }, () =>
            {
                _inputDispatcher.UnblockInputs();
                _isDodging = false;
            });
            _animationController.Dodge(Physics.Velocity);
        }

        private bool CanAttack()
        {
            return !_isDodging && !_isJumping && !_inputDispatcher.IsDeffending && !_inputDispatcher.IsRunning;
        }

        private bool CanDeffend()
        {
            return !_isDodging && !_isJumping && !_inputDispatcher.IsRunning;
        }

        private void Jump(Vector2 dir)
        {
            if (Physics.Velocity.magnitude <= 0)
                return;

            _inputDispatcher.BlockInputs();
            _isJumping = true;
            _animationController.Jump(_isJumping);

            Physics.Jump(dir.normalized, () => { transform.position = Physics.Position; }, () =>
            {
                _inputDispatcher.UnblockInputs();
                _isJumping = false;
                _animationController.Jump(_isJumping);
            });
        }

        private void Stun()
        {
            ConsoleDebug.LogError("Stun");
        }

        private void Die()
        {
            ConsoleDebug.LogError("Die");
        }

        private void MapToggle()
        {
            if (_mapOpen)
            {
                OpenMap();
                _mapOpen = false;
            }
            else
            {
                CloseMap();
                _mapOpen = true;
            }
        }

        private void OpenMap()
        {
            // _inputDispatcher.BlockInputs();
            if (OnOpenMap != null) OnOpenMap();
        }

        private void CloseMap()
        {
            // _inputDispatcher.UnblockInputs();
            if (OnCloseMap != null) OnCloseMap();
        }

        private void Interact()
        {
            if(!_canInteract || _interactible == null) return;
            
            _interactible.Interact();
            _interactible = null;
        }

        
        public void DoDamage(int damage)
        {
            if (_isDodging || Time.time <= _lastHit + _invencibilityDuration) return;

            _lastHit = Time.time;

            Life.ReceiveDamage(damage);
        }
    }
}