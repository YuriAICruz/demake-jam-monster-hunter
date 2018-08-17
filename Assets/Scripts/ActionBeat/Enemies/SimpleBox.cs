using Shooter;
using UnityEngine;

namespace ActionBeat.Enemies
{
    public class SimpleBox : MonoBehaviour, IDamageble
    {
        public void DoDamage(int damage)
        {
            Debug.Log("Damage " + damage);
        }
    }
}