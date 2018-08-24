using System;
using UnityEngine;

namespace ActionBeat
{
    public class ActionGameManagement : MonoBehaviour
    {
        public Action StartGame;
        public Action EndGame;
        public Action GameOver;

        public AudioClip Victory, Game;

        private AudioSource _source;

        private void Start()
        {
            _source = GetComponent<AudioSource>();
            _source.Stop();

            StartGame += PlayGameSound;
            EndGame += PlayGameEndSound;
        }

        private void PlayGameSound()
        {
            _source.clip = Game;
            _source.Play();
        }
        private void PlayGameEndSound()
        {
            _source.clip = Victory;
            _source.Play();
        }
    }
}