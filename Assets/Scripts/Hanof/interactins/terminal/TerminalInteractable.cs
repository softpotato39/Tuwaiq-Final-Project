using UnityEngine;

namespace InteractionSystem
{
    /// <summary>
    /// Put this on the terminal's collider along with a TerminalSession (same object or a
    /// parent/child - either works since GetComponent is used directly here).
    /// </summary>
    [RequireComponent(typeof(TerminalSession))]
    public class TerminalInteractable : MonoBehaviour, IInteractable
    {
        [Tooltip("World-space icon/GameObject shown while this is the look-at target.")]
        [SerializeField] private GameObject promptIcon;

        private TerminalSession _session;

        private void Awake() => _session = GetComponent<TerminalSession>();

        public void ShowPrompt() => promptIcon?.SetActive(true);

        public void HidePrompt() => promptIcon?.SetActive(false);

        public bool CanInteract(PlayerInteractor interactor) =>
            TerminalController.Instance != null && !TerminalController.Instance.IsActive;

        public void Interact(PlayerInteractor interactor) => TerminalController.Instance.EnterTerminal(_session);
    }
}
