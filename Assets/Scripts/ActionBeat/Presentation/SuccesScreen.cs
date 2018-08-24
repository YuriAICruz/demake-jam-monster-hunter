using UiGenerics;

namespace ActionBeat.Presentation
{
    public class SuccesScreen : CanvasGroupView
    {
        private ActionGameManagement _manager;

        void Setup()
        {
            Hide();
        }

        private void Start()
        {
            _manager = FindObjectOfType<ActionGameManagement>();

            _manager.EndGame += ShowMe;
        }

        private void ShowMe()
        {
            Show();
            Invoke("Reload", 3);
        }

        void Reload()
        {
            SceneManagement.SceneManager.LoadScene(0);
        }
    }
}