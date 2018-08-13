using Shooter;
using UiGenerics;
using UnityEngine;

namespace ActionBeat.Presentation
{
    public class StaminaBar : ImageView
    {
        private Stamina _stamina;

        void Setup(){}
        
        private void Start()
        {
            FindPlayerCharacter();
        }

        private void FindPlayerCharacter()
        {
            _stamina = FindObjectOfType<ZeldaLikeCharacter>().Stamina;
        }

        void Update()
        {
            Image.fillAmount = _stamina.Stm / _stamina.MaxStm;
        }
    }
}