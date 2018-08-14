using UiGenerics;

namespace ActionBeat.Presentation
{
    public class GroundInfo : TextView
    {
        private ZeldaLikeCharacter _player;

        void Setup()
        {
            
        }

        void Start()
        {
            _player = FindObjectOfType<ZeldaLikeCharacter>();
        }

        private void Update()
        {
            Text.text = _player.Physics.GetGround().ToString();
        }
    }
}