using UnityEngine;

//////////////////////////////////////////////
//                                          //
//       This script shall live on:         //
//            The Terminal :O               //
//                                          //
//////////////////////////////////////////////

// script's purpose: allows the player to turn on and off the screen for
//                   the terminal once interacted with from the script:
//                   TerminalInteractable.

// script's requirements: Collider (any), Prompt, interact button, sounds on/off, screen on/of mesh and Cinemachine cam !

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    /// <summary>
    /// Put this on the terminal's collider along with a TerminalSession (same object or a
    /// parent/child - either works since GetComponent is used directly here).
    /// </summary>
    [RequireComponent(typeof(TerminalSession))]
    public class TerminalInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject promptIcon;     // UI prompt showing up to interact

        private TerminalSession _session;

        private void Awake() => _session = GetComponent<TerminalSession>();

        public void ShowPrompt() => promptIcon?.SetActive(true);

        public void HidePrompt() => promptIcon?.SetActive(false);

        public bool CanInteract(PlayerInteractor interactor) =>
            TerminalController.Instance != null && !TerminalController.Instance.IsActive;

        public void Interact(PlayerInteractor interactor) => TerminalController.Instance.EnterTerminal(_session);
    }
}
