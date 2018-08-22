using System.IO;
using System.Linq;
using UnityEngine;

namespace CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public class CameraManagement : MonoBehaviour
    {
        private Player _player;

        private CameraBounds _bounds;

        public float Margin = 2;

        private Camera _camera;

        public Path Path;
        public float Speed;

        public void Awake()
        {
            _camera = GetComponent<Camera>();
            _bounds = new CameraBounds(_camera, transform, Margin);
        }

        private void Start()
        {
            FindPlayer();
        }

        public void StartMove()
        {
            Path.MoveOnPath(Speed, transform, this);
        }

        private void FindPlayer()
        {
            var plrs = FindObjectsOfType<Player>().ToList();

            if (plrs.Count == 1)
            {
                _player = plrs[0];
            }
            else if (plrs.Count > 1)
            {
                _player = plrs.Find(x => x.IsOwner);
            }

            if (_player != null)
            {
                _player.Life.OnDie += Stop;
            }
        }

        private void Stop()
        {
            Path.Stop();
        }

        private void Update()
        {
            if (_player == null)
            {
                FindPlayer();
                return;
            }
        }

        public Vector2 GetTopLeftBorder(Vector3 pos)
        {
            return _bounds.GetTopLeftBorder(pos);
        }

        public Vector2 GetBottonRightBorder(Vector3 pos)
        {
            return _bounds.GetBottonRightBorder(pos);
        }

        public bool GetInsideBounds(Vector3 pos)
        {
            return _bounds.GetInsideBounds(pos);
        }
    }
}