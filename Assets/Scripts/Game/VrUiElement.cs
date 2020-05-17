using UnityEngine;

namespace Game
{
    public enum UiAction
    {
        Start,
        Exit,
        Credits,
        Settings,
        CanvasHover
    }

    public class VrUiElement : MonoBehaviour
    {
        [SerializeField] private UiAction action;

        [SerializeField] private GameObject unselectedPanel;
        [SerializeField] private GameObject hoverPanel;

        private bool _hover;

        private void Start()
        {
            if ((!unselectedPanel || !hoverPanel) && action != UiAction.CanvasHover)
                Debug.LogError("Set the UI panels!");
        }

        public void SetHoverState(bool isHoveringOver)
        {
            if (_hover && isHoveringOver || !_hover && !isHoveringOver) return;

            _hover = isHoveringOver;

            if (action == UiAction.CanvasHover) return;

            hoverPanel.SetActive(isHoveringOver);
            unselectedPanel.SetActive(!isHoveringOver);
        }

        public void ExecuteAction()
        {
            switch (action)
            {
                case UiAction.Start:
                    GameHandler.Instance.StartGame();
                    break;
                case UiAction.Exit:
                    Application.Quit();
                    break;
                case UiAction.Credits:
                    GameSettings.Instance.DisplayCredits();
                    break;
                case UiAction.Settings:
                    GameSettings.Instance.DisplaySettings();
                    break;
            }
        }

        public void ToggleActivation(bool active)
        {
            unselectedPanel.transform.parent.gameObject.SetActive(active);
            GetComponent<Collider>().enabled = active;
        }
    }
}