using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//             The Player ! :D              //
//                                          //
//////////////////////////////////////////////

// script's purpose: using raycast from the player camera, detect interactables
//                   and show UI prompts. can be used for interact or hold to
//                   interact or hold to interact. also tracks held items !

// script's requirements: The player's input map.

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Camera playaCam;                   // player camera :3
        [SerializeField] private float interactRange = 3f;          // how far the player can reach
        [SerializeField] private float interactRadius = 0.15f;      // how much the player has to turn to interact
        [SerializeField] private LayerMask interactableMask = ~0;   // the mask on things we want to have be interactable

        [Header("Input")]
        [SerializeField] private InputActionReference interactAction;

        [Header("Carry Point")]
        [SerializeField] private Transform handSocket;              // this is the empty hand place for the player

        [Header("Events")]
        public UnityEvent<float> OnHoldProgressChanged;             // this is for the progress for hold to interact, starts 0 goes to 1. we can use it for UI

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

        private void LateUpdate()   // late update cuz it makes the prompt UI less glitchy
        {
            ScanForInteractable();

            if (_isHolding)
                TickHold();
        }

        private void ScanForInteractable()
        {

            // this is to fix a bug where once u pick up an item, u cant pick up a second one :)
            if (_currentTarget != null && _currentTarget as UnityEngine.Object == null)
                _currentTarget = null;

            IInteractable found = null;

            Ray ray = playaCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            bool hit = interactRadius > 0f
                ? Physics.SphereCast(ray, interactRadius, out RaycastHit info, interactRange, interactableMask, QueryTriggerInteraction.Collide)
                : Physics.Raycast(ray, out info, interactRange, interactableMask, QueryTriggerInteraction.Collide);

            if (hit)
                found = info.collider.GetComponentInParent<IInteractable>();

            // this is for if the player looks away or lets go of hold, we make them restart >:)
            if (_isHolding && found != _currentTarget)
                CancelHold();

            if (found != _currentTarget)
            {
                // safety check for hiding the prompt :)
                _currentTarget?.HidePrompt();

                _currentTarget = found;
                _currentHoldTarget = found as IHoldInteractable;

                if (_currentTarget != null && _currentTarget.CanInteract(this))
                    _currentTarget.ShowPrompt();
            }
        }

        //private void OnInteractPerformed(InputAction.CallbackContext ctx)
        //{
        //    if (_currentTarget == null || !_currentTarget.CanInteract(this))
        //        return;

        //    if (_currentHoldTarget != null)
        //    {
        //        _isHolding = true;
        //        _holdTimer = 0f;
        //    }
        //    else
        //    {
        //        _currentTarget.Interact(this);

        //        // safety check for hiding the prompt :)
        //        if (!_currentTarget.CanInteract(this))
        //            _currentTarget.HidePrompt();
        //    }
        //}

        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            // looking at a interactable (trash) — takes priority
            if (_currentTarget != null && _currentTarget.CanInteract(this))
            {
                if (_currentHoldTarget != null)
                {
                    _isHolding = true;
                    _holdTimer = 0f;
                }
                else
                {
                    _currentTarget.Interact(this);

                    if (!_currentTarget.CanInteract(this))
                        _currentTarget.HidePrompt();
                }
                return;
            }

            // nothing to interact with — drop carried item ifff holding one
            if (IsCarryingItem)
            {
                PickupItem item = CurrentItem;
                ClearCarriedItem();
                item.transform.SetParent(null, true);
                item.Drop(GetComponent<Collider>());
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

                // safety check for hiding the prompt :)
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

        // this tracks any "carried" items :) [used in pickupitem and trash scripts]

        public void SetCarriedItem(PickupItem item) => CurrentItem = item;

        public void ClearCarriedItem() => CurrentItem = null;
    }
}
