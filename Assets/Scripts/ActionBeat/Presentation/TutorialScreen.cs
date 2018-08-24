using TMPro;
using UiGenerics;

namespace ActionBeat.Presentation
{
    public class TutorialScreen : CanvasGroupView
    {
        private ActionGameManagement _manager;
        
        

        void Setup()
        {
            Hide();
        }

        private void Start()
        {
            _manager = FindObjectOfType<ActionGameManagement>();

            _manager.StartGame += ShowMe;
        }

        private void ShowMe()
        {
            Show();
            Invoke("Hide", 2);
        }
    }
}