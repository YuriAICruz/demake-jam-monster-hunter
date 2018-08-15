using System.Collections.Generic;
using ActionBeat.Animation;
using Debuging;
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
        private bool _isJumping;

        private void Awake()
        {
            Life.Reset();
            Life.OnDie += Die;

            Stamina.Reset();
            Stamina.Stun += Stun;
        }

        void Start()
        {
            _animationController = new AnimationController(transform, GetComponent<Animator>());

            Physics.SetPosition(transform.position);
            Physics.SetCollider(GetComponent<Collider2D>());
            Physics.OnCollisionEnter += OnCollided;
            Physics.OnTriggerEnter += OnTriggered;
            Physics.OnJump += Jump;
        }

        private void OnEnable()
        {
            if (InputDispatcher == null)
                InputDispatcher = new ZeldaLikeInputDispatcher(this);

            InputDispatcher.OverheadSlash += OverheadSlash;
            InputDispatcher.RisingSlash += RisingSlash;
            InputDispatcher.ChargedSlash += ChargedSlash;
            InputDispatcher.WideSlash += WideSlash;
            InputDispatcher.TrueChargedSlashCombo += TrueChargedSlashCombo;
            InputDispatcher.TrueChargedSlashComboFinal += TrueChargedSlashComboFinal;
            InputDispatcher.FowardLungingAttackCombo += FowardLungingAttackCombo;
            InputDispatcher.FowardLungingAttackComboFinal += FowardLungingAttackComboFinal;
            InputDispatcher.StationaryCombo += StationaryCombo;
            InputDispatcher.StationaryComboFinal += StationaryComboFinal;
            InputDispatcher.Deffend += Deffend;
            InputDispatcher.Dodge += Dodge;
            InputDispatcher.LeftStick += Move;
        }

        private void OnDisable()
        {
            InputDispatcher.OverheadSlash -= OverheadSlash;
            InputDispatcher.RisingSlash -= RisingSlash;
            InputDispatcher.ChargedSlash -= ChargedSlash;
            InputDispatcher.WideSlash -= WideSlash;
            InputDispatcher.TrueChargedSlashCombo -= TrueChargedSlashCombo;
            InputDispatcher.FowardLungingAttackCombo -= FowardLungingAttackCombo;
            InputDispatcher.StationaryCombo -= StationaryCombo;
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


        private void WideSlash()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("WideSlash");
            _animationController.WideSlash();
        }

        private void StationaryCombo()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("StationaryCombo");
            _animationController.StationaryCombo();
        }

        private void FowardLungingAttackCombo()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("FowardLungingAttackCombo");
            _animationController.FowardLungingAttackCombo();
        }

        private void TrueChargedSlashCombo()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("TrueChargedSlashCombo");
            _animationController.TrueChargedSlashCombo();
        }

        private void ChargedSlash()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("ChargedSlash");
            _animationController.ChargedSlash();
        }

        private void RisingSlash()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("RisingSlash");
            _animationController.RisingSlash();
        }

        private void OverheadSlash()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("OverheadSlash");
            _animationController.OverheadSlash();
        }

        private void StationaryComboFinal()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("StationaryComboFinal");
            _animationController.StationaryComboFinal();
        }

        private void FowardLungingAttackComboFinal()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("FowardLungingAttackComboFinal");
            _animationController.FowardLungingAttackComboFinal();
        }

        private void TrueChargedSlashComboFinal()
        {
            if (!CanAttack()) return;
            ConsoleDebug.Log("TrueChargedSlashComboFinal");
            _animationController.TrueChargedSlashComboFinal();
        }

        private void Deffend(bool isOn)
        {
            if (!CanDeffend()) return;
            _animationController.Deffend(isOn);
        }


        private void OnTriggered(RaycastHit2D obj)
        {
        }

        private void OnCollided(RaycastHit2D obj)
        {
        }

        private void Dodge()
        {
            if (InputDispatcher.IsDeffending || Physics.Velocity.magnitude <= 0)
                return;

            Stamina.DoAction(5);

            InputDispatcher.BlockInputs();
            _isDodging = true;
            Physics.Dodge(() => { transform.position = Physics.Position; }, () =>
            {
                InputDispatcher.UnblockInputs();
                _isDodging = false;
            });
            _animationController.Dodge(Physics.Velocity);
        }

        bool CanAttack()
        {
            return !_isDodging && !_isJumping && !InputDispatcher.IsDeffending && !InputDispatcher.IsRunning;
        }

        private bool CanDeffend()
        {
            return !_isDodging && !_isJumping && !InputDispatcher.IsRunning;
        }

        private void Jump(Vector2 dir)
        {
            if (Physics.Velocity.magnitude <= 0)
                return;

            InputDispatcher.BlockInputs();
            _isJumping = true;
            _animationController.Jump(_isJumping);

            Physics.Jump(dir.normalized, () => { transform.position = Physics.Position; }, () =>
            {
                InputDispatcher.UnblockInputs();
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
    }
}