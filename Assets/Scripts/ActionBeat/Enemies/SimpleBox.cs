using System.Xml;
using ActionBeat.Presentation;
using Shooter;
using UnityEngine;

namespace ActionBeat.Enemies
{
    public class SimpleBox : MonoBehaviour, IDamageble
    {
        public void DoDamage(int damage)
        {
            InstantiateDamage(damage);
        }

        void InstantiateDamage(int damage)
        {
            var obj = Instantiate(Resources.Load<DamageInfo>("Damage"));
            obj.SetDamage(damage, transform.position);
        }
    }
}