using UnityEngine;

namespace ActionBeat
{
    public class BasicCamera : MonoBehaviour
    {
        private ZeldaLikeCharacter _player;

        private Vector3 _position;

        private void Start()
        {
            _player = FindObjectOfType<ZeldaLikeCharacter>();
            GetPosition();
        }

        private void GetPosition()
        {
            _position = _player.transform.position;
            _position.z = transform.position.z;
        }

        void Update()
        {
            GetPosition();
            transform.position = _position;
        }
    }
}