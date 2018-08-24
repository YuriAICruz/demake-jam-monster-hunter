using TMPro;
using UiGenerics;

namespace ActionBeat.Presentation
{
    public class GameOverScreen : CanvasGroupView
    {
        private ActionGameManagement _manager;

        void Setup()
        {
            Hide();
        }

        private void Start()
        {
            _manager = FindObjectOfType<ActionGameManagement>();

            _manager.StartGame += Hide;
            _manager.GameOver += Show;
        }
    }
}