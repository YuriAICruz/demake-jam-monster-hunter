using UiGenerics;
using UnityEngine;

namespace ActionBeat.Presentation
{
    public class Duration : TextView
    {
        private ActionGameManagement _manager;
        private float _startTime;

        void Setup()
        {
        }

        private void Start()
        {
            _manager = FindObjectOfType<ActionGameManagement>();

            _manager.StartGame += ResetTime;
        }

        private void ResetTime()
        {
            _startTime = Time.time;
        }

        private void Update()
        {
            Text.text = (Time.time - _startTime).ToString("00.00");
        }
    }
}