using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//          Terminal Canvas UI              //
//                                          //
//////////////////////////////////////////////

// script's purpose: reads a simple up and down input from the player and 
//                   renders it on the canvas UI for the terminal !

// script's requirements: just set up the canvas UI correctly and u'll be okay :)

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track

{
    public class TerminalMenuNavigator : MonoBehaviour
    {
        [SerializeField] private InputActionReference navigateAction;   // assign me in inspector ! :)
        [SerializeField] private InputActionReference confirmAction;    // assign me in inspector ! :3
        [SerializeField] private Button[] menuItems;
        [SerializeField] private float repeatDelay = 0.25f;     // this makes it so when using joystick a single flick doesnt fly through the whole menu

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
                // oh god its math 

                int dir = nav.y > 0f ? -1 : 1; 
                // ^ this is so when the player goes up it goes to the previous item
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
