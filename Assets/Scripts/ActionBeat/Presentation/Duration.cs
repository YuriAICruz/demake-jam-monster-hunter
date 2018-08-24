using UiGenerics;
using UnityEngine;

namespace ActionBeat.Presentation
{
    public class Duration : TextView
    {
        private ActionGameManagement _manager;
        private float _startTime;
        private float _endTime;
        private bool _ended;

        void Setup()
        {
        }

        private void Start()
        {
            _manager = FindObjectOfType<ActionGameManagement>();

            _manager.StartGame += ResetTime;
            _manager.EndGame += Endgame;
            _manager.GameOver += Endgame;
        }

        private void Endgame()
        {
            _ended = true;
        }

        private void ResetTime()
        {
            _startTime = Time.time;
        }

        private void Update()
        {
            if (!_ended)
                _endTime = Time.time;
            
            Text.text = (_endTime - _startTime).ToString("00.00");
        }
    }
}