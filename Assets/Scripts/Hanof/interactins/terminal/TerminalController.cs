using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//            The Player ! :D               //
//                                          //
//////////////////////////////////////////////

// script's purpose: It allows the player to transition from normal cam view
//                   to the terminal cam view. Locking the walk and stuff and
//                   making the player only interact with the terminal until 
//                   they choose to exit

// script's requirements: Input map Player/Terminal, PlayerControlLocker script on player.

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    public class TerminalController : MonoBehaviour
    {
        public static TerminalController Instance { get; private set; }

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;                   // assign the input map u use here ! :)
        [SerializeField] private string playerMapName = "Player";           // the name of the normal player input map
        [SerializeField] private string terminalMapName = "Terminal";       // the name of the terminal input map
        [SerializeField] private InputActionReference terminalExitAction;   // the button to press if u wanna exit

        [Header("Player Lock")]
        [SerializeField] private PlayerControlLocker controlLocker;

        [Header("Camera")]
        [SerializeField] private int terminalCameraPriority = 20;
        [SerializeField] private int defaultCameraPriority = 0;

        [Header("Events")]
        public UnityEvent<TerminalSession> OnTerminalEntered;
        public UnityEvent OnTerminalExited;

        public bool IsActive { get; private set; }

        private TerminalSession _activeSession;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (terminalExitAction != null)
                terminalExitAction.action.performed += OnExitPerformed;
        }

        private void OnDisable()
        {
            if (terminalExitAction != null)
                terminalExitAction.action.performed -= OnExitPerformed;
        }

        public void EnterTerminal(TerminalSession session)
        {
            if (IsActive || session == null) return;

            IsActive = true;
            _activeSession = session;

            session.PowerOn();

            if (session.Camera != null)
                session.Camera.Priority = terminalCameraPriority;

            controlLocker?.LockPlayerControls();
            playerInput.SwitchCurrentActionMap(terminalMapName);

            OnTerminalEntered?.Invoke(session);
        }

        public void ExitTerminal()
        {
            if (!IsActive) return;

            if (_activeSession != null && _activeSession.Camera != null)
                _activeSession.Camera.Priority = defaultCameraPriority;

            playerInput.SwitchCurrentActionMap(playerMapName);
            controlLocker?.UnlockPlayerControls();

            IsActive = false;
            _activeSession = null;

            OnTerminalExited?.Invoke();
        }

        private void OnExitPerformed(InputAction.CallbackContext ctx)
        {
            if (IsActive) ExitTerminal();
        }
    }
}
