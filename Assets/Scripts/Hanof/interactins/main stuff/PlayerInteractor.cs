using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace InteractionSystem
{
    // this script stays on: THE PLAYER
    //using raycast from the player camera, detect interactables and show UI :3
    // can be used for interact or hold to interact
    // also tracks held items
    [DisallowMultipleComponent]
    public class PlayerInteractor : MonoBehaviour
    {
        //all these "serializedField" things are to make the code private and secure, but still show it in unity
        [Header("Detection")]
        [SerializeField] private Camera playaCam;   // player camera :3
        [SerializeField] private float interactRange = 3f; 
        [SerializeField] private float interactRadius = 0.15f;
        [SerializeField] private LayerMask interactableMask = ~0;

        [Header("Input")]
        [SerializeField] private InputActionReference interactAction;

        [Header("Carry Point")]
        [SerializeField] private Transform handSocket;      //this is the empty hand place for the player

        [Header("Events")]
        public UnityEvent<float> OnHoldProgressChanged; //this is for the progress for hold to interact, starts 0 goes to 1. we can use it for UI

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

        private void LateUpdate()
        {
            ScanForInteractable();

            if (_isHolding)
                TickHold();
        }

        private void ScanForInteractable()
        {

            // this is to fix a bug where once u pick up an item, u cant pick up a second one
            if (_currentTarget != null && _currentTarget as UnityEngine.Object == null)
                _currentTarget = null;

            IInteractable found = null;

            Ray ray = playaCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
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

        // --- carry here is used by PickupItem / TrashBin script :3 ---

        public void SetCarriedItem(PickupItem item) => CurrentItem = item;

        public void ClearCarriedItem() => CurrentItem = null;
    }
}
