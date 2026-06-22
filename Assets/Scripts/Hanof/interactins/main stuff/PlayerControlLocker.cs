using UnityEngine;
using UnityEngine.Events;

namespace InteractionSystem
{
    /// <summary>
    /// Bridges this interaction system to your existing movement/camera-look scripts.
    /// Drag your movement script(s) into "Behaviours To Disable" in the inspector - they get
    /// turned off whenever something locks player controls (currently just TerminalController,
    /// but anything can call LockPlayerControls/UnlockPlayerControls) and restored after.
    /// </summary>
    public class PlayerControlLocker : MonoBehaviour
    {
        [Tooltip("Your movement / camera-look / etc. MonoBehaviours. Disabled on lock, re-enabled on unlock.")]
        [SerializeField] private Behaviour[] behavioursToDisable;

        [Tooltip("Optional: free the hardware cursor while locked (handy if your terminal UI needs mouse navigation instead of WASD/joystick).")]
        [SerializeField] private bool manageCursor = false;

        public UnityEvent OnLocked;
        public UnityEvent OnUnlocked;

        public bool IsLocked { get; private set; }

        public void LockPlayerControls()
        {
            if (IsLocked) return;
            IsLocked = true;

            foreach (var b in behavioursToDisable)
                if (b != null) b.enabled = false;

            if (manageCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            OnLocked?.Invoke();
        }

        public void UnlockPlayerControls()
        {
            if (!IsLocked) return;
            IsLocked = false;

            foreach (var b in behavioursToDisable)
                if (b != null) b.enabled = true;

            if (manageCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            OnUnlocked?.Invoke();
        }
    }
}
