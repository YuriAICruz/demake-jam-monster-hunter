using TMPro;
using UiGenerics;

namespace ActionBeat.Presentation
{
    public class TittleScreen : CanvasGroupView
    {
        private ActionGameManagement _manager;

        void Setup()
        {
            Show();
        }

        private void Start()
        {
            _manager = FindObjectOfType<ActionGameManagement>();

            _manager.StartGame += Hide;
        }
    }
}