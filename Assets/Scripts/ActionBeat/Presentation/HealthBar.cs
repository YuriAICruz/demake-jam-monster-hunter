using Shooter;
using UiGenerics;
using UnityEngine;
using UnityEngine.UI;

namespace ActionBeat.Presentation
{
    public class HealthBar : ImageView
    {
        private Life _life;
        
        void Setup(){}

        private void Start()
        {
            FindPlayerCharacter();
        }

        private void FindPlayerCharacter()
        {
            _life = FindObjectOfType<ZeldaLikeCharacter>().Life;
        }

        void Update()
        {
            Image.fillAmount = _life.Hp / (float) _life.MaxHp;
        }
    }
}