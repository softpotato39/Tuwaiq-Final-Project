using UnityEngine;

namespace InteractionSystem
{
    /// <summary>
    /// Anything the player can pick up and carry to a destination (e.g. TrashBin).
    /// On pickup it parents itself to the player's hand socket and disables its own
    /// physics/colliders so it rides along cleanly.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [Tooltip("Used by destinations (e.g. TrashBin) that only want to accept specific item types. Leave blank if unused.")]
        [SerializeField] private string itemId = "GenericTrash";
        [Tooltip("World-space icon/GameObject shown while this is the look-at target, e.g. a billboarded sprite reading 'Pick Up'.")]
        [SerializeField] private GameObject promptIcon;
        [SerializeField] private bool disableCollidersWhileCarried = true;

        public string ItemId => itemId;

        private Collider[] _colliders;
        private Rigidbody _rb;

        private void Awake()
        {
            _colliders = GetComponentsInChildren<Collider>();
            _rb = GetComponent<Rigidbody>();
        }

        public void ShowPrompt() => promptIcon?.SetActive(true);

        public void HidePrompt() => promptIcon?.SetActive(false);

        // Keeping it simple: one carried item at a time. Relax this if you want a real inventory.
        public bool CanInteract(PlayerInteractor interactor) => !interactor.IsCarryingItem;

        public void Interact(PlayerInteractor interactor)
        {
            Transform socket = interactor.HandSocket;

            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.useGravity = false;
            }

            if (disableCollidersWhileCarried)
                foreach (var c in _colliders)
                    c.enabled = false;

            transform.SetParent(socket, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            interactor.SetCarriedItem(this);
        }
    }
}
