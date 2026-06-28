using UnityEngine;
using UnityEngine.Events;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//             The Player ! :D              //
//                                          //
//////////////////////////////////////////////

// script's purpose: this script locks the player movements and controls
//                   the unlocks them. i need it for the terminal stuff
//                   but i'm not done figuring that out yet so ignore it :)

// script's requirements: The player's input map.

namespace InteractionSystem    // <-- this is for unity so it groups classes together without losing track
{
    public class PlayerControlLocker : MonoBehaviour
    {
        [SerializeField] private Behaviour[] behavioursToDisable;   // the input we want to stop
        [SerializeField] private bool manageCursor = false;         // this can enable the cursor if we want :)

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
