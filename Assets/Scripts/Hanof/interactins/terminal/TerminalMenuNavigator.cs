using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace InteractionSystem
{
    /// <summary>
    /// OPTIONAL EXAMPLE - not required by the rest of the system. Shows the input-wiring
    /// pattern for driving a vertical list of UI Buttons with the Terminal map's Navigate
    /// (Vector2) and Confirm (Button) actions. Swap this out for your own terminal UI/menu
    /// system once you've seen the pattern; it deliberately stays dumb and minimal.
    /// </summary>
    public class TerminalMenuNavigator : MonoBehaviour
    {
        [SerializeField] private InputActionReference navigateAction;
        [SerializeField] private InputActionReference confirmAction;
        [SerializeField] private Button[] menuItems;
        [Tooltip("Seconds between repeat moves while the stick stays tilted, so one tilt doesn't fly through the whole list.")]
        [SerializeField] private float repeatDelay = 0.25f;

        private int _selectedIndex;
        private float _repeatTimer;

        private void OnEnable()
        {
            navigateAction.action.Enable();
            confirmAction.action.Enable();
            confirmAction.action.performed += OnConfirm;
            if (menuItems.Length > 0) Highlight(0);
        }

        private void OnDisable()
        {
            confirmAction.action.performed -= OnConfirm;
        }

        private void Update()
        {
            if (menuItems.Length == 0) return;

            Vector2 nav = navigateAction.action.ReadValue<Vector2>();
            _repeatTimer -= Time.unscaledDeltaTime;

            if (Mathf.Abs(nav.y) > 0.5f && _repeatTimer <= 0f)
            {
                int dir = nav.y > 0f ? -1 : 1; // up = previous item
                Highlight((_selectedIndex + dir + menuItems.Length) % menuItems.Length);
                _repeatTimer = repeatDelay;
            }
            else if (Mathf.Abs(nav.y) < 0.2f)
            {
                _repeatTimer = 0f;
            }
        }

        private void Highlight(int index)
        {
            _selectedIndex = index;
            menuItems[_selectedIndex].Select();
        }

        private void OnConfirm(InputAction.CallbackContext ctx)
        {
            if (menuItems.Length == 0) return;
            menuItems[_selectedIndex].onClick.Invoke();
        }
    }
}
