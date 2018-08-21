using UiGenerics;

namespace ActionBeat.Presentation
{
    public class Map : CanvasGroupView
    {
        private ZeldaLikeCharacter _player;

        private void Setup()
        {
            Hide();
        }

        private void Start()
        {
            _player = FindObjectOfType<ZeldaLikeCharacter>();
            _player.OnCloseMap += CloseMap;
            _player.OnOpenMap += OpenMap;
        }

        private void OpenMap()
        {
            Show();
        }

        private void CloseMap()
        {
            Hide();
        }
    }
}