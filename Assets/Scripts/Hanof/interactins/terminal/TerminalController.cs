using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace InteractionSystem
{
    /// <summary>
    /// One of these lives on the Player. It owns the transition into/out of "terminal mode":
    /// powering on the screen, focusing the Cinemachine camera, swapping the active Input
    /// Actions map from Player to Terminal (WASD/joystick = menu navigation, a Confirm
    /// button, an Exit button), and locking the normal player controller via PlayerControlLocker.
    /// </summary>
    public class TerminalController : MonoBehaviour
    {
        public static TerminalController Instance { get; private set; }

        [Header("Input")]
        [Tooltip("The PlayerInput component on the player, used to switch the active action map.")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string playerMapName = "Player";
        [SerializeField] private string terminalMapName = "Terminal";
        [Tooltip("The Exit/Cancel action living inside the Terminal map (Escape / gamepad East button).")]
        [SerializeField] private InputActionReference terminalExitAction;

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
