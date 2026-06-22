using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace InteractionSystem
{
    /// <summary>
    /// Lives on the player. Casts a ray from the centre of the camera each frame to find the
    /// IInteractable being looked at, fires prompt-change events for UI, and routes the
    /// Interact input action into either a single Interact() call or a tracked hold sequence
    /// (for anything implementing IHoldInteractable). Also tracks the item currently being
    /// carried, which PickupItem and TrashBin read/write.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactRange = 3f;
        [Tooltip("0 = pure raycast. >0 = SphereCast, more forgiving for small/thin objects.")]
        [SerializeField] private float interactRadius = 0.15f;
        [SerializeField] private LayerMask interactableMask = ~0;

        [Header("Input")]
        [Tooltip("A Button-type action, e.g. E / Gamepad South button.")]
        [SerializeField] private InputActionReference interactAction;

        [Header("Carry Point")]
        [Tooltip("Empty transform on the player (e.g. under the camera or a hand bone) where picked-up items get parented.")]
        [SerializeField] private Transform handSocket;

        [Header("Events")]
        [Tooltip("Fires 0..1 while holding Interact on an IHoldInteractable, and resets to 0 on release/cancel/complete.")]
        public UnityEvent<float> OnHoldProgressChanged;

        public PickupItem CurrentItem { get; private set; }
        public Transform HandSocket => handSocket;
        public bool IsCarryingItem => CurrentItem != null;

        private IInteractable _currentTarget;
        private IHoldInteractable _currentHoldTarget;
        private bool _isHolding;
        private float _holdTimer;

        private void OnEnable()
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
            interactAction.action.canceled += OnInteractCanceled;
        }

        private void OnDisable()
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.canceled -= OnInteractCanceled;
            interactAction.action.Disable();

            if (_isHolding) CancelHold();
        }

        private void Update()
        {
            ScanForInteractable();

            if (_isHolding)
                TickHold();
        }

        private void ScanForInteractable()
        {
            IInteractable found = null;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            bool hit = interactRadius > 0f
                ? Physics.SphereCast(ray, interactRadius, out RaycastHit info, interactRange, interactableMask, QueryTriggerInteraction.Collide)
                : Physics.Raycast(ray, out info, interactRange, interactableMask, QueryTriggerInteraction.Collide);

            if (hit)
                found = info.collider.GetComponentInParent<IInteractable>();

            // Looked away mid-hold -> cancel rather than silently keep counting.
            if (_isHolding && found != _currentTarget)
                CancelHold();

            if (found != _currentTarget)
            {
                // Only the look-at TRANSITION is handled here. If an interactable's own state
                // changes its prompt validity without the target changing (e.g. a hold-interactable
                // completing while still being looked at), it's responsible for calling its own
                // HidePrompt() - see TransformInteractable.OnHoldComplete for an example.
                _currentTarget?.HidePrompt();

                _currentTarget = found;
                _currentHoldTarget = found as IHoldInteractable;

                if (_currentTarget != null && _currentTarget.CanInteract(this))
                    _currentTarget.ShowPrompt();
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            if (_currentTarget == null || !_currentTarget.CanInteract(this))
                return;

            if (_currentHoldTarget != null)
            {
                _isHolding = true;
                _holdTimer = 0f;
            }
            else
            {
                _currentTarget.Interact(this);

                // Same target, but its own CanInteract may now read false (e.g. you just picked
                // up the only item it accepts). The interactable is also free to call its own
                // HidePrompt() directly inside Interact() if it wants tighter control over this.
                if (!_currentTarget.CanInteract(this))
                    _currentTarget.HidePrompt();
            }
        }

        private void OnInteractCanceled(InputAction.CallbackContext ctx)
        {
            if (_isHolding)
                CancelHold();
        }

        private void TickHold()
        {
            if (_currentHoldTarget == null)
            {
                CancelHold();
                return;
            }

            _holdTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_holdTimer / Mathf.Max(0.01f, _currentHoldTarget.HoldDuration));
            _currentHoldTarget.OnHoldProgress(t);
            OnHoldProgressChanged?.Invoke(t);

            if (t >= 1f)
            {
                _currentHoldTarget.OnHoldComplete(this);
                _isHolding = false;
                OnHoldProgressChanged?.Invoke(0f);

                // Same safety net as the instant-interact path above - belt and braces in case
                // the interactable's own OnHoldComplete didn't already hide its prompt.
                if (!_currentTarget.CanInteract(this))
                    _currentTarget.HidePrompt();
            }
        }

        private void CancelHold()
        {
            _currentHoldTarget?.OnHoldCancelled();
            _isHolding = false;
            OnHoldProgressChanged?.Invoke(0f);
        }

        // --- Carry API used by PickupItem / TrashBin ---

        public void SetCarriedItem(PickupItem item) => CurrentItem = item;

        public void ClearCarriedItem() => CurrentItem = null;
    }
}
