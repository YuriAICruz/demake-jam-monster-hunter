using UiGenerics;

namespace ActionBeat.Presentation
{
    public class StartGame : ButtonView
    {
        private ActionGameManagement _manager;

        void Setup()
        {
            
        }

        private void Start()
        {
            _manager = FindObjectOfType<ActionGameManagement>();
        }

        protected override void OnClick()
        {
            base.OnClick();

            _manager.StartGame();
        }
    }
}