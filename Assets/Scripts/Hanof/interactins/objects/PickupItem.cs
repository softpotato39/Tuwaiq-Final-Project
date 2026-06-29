using UnityEngine;
using System.Collections;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//          Pickupable stuff ! :D           //
//                                          //
//////////////////////////////////////////////

// script's purpose: lets players see a the desired prompt once raycast reaches the item, pick the
//                   item by interaction action from the input system. Which then disables the
//                   colliders and rigidbody of the item so it doesnt ruin things :)

// script's requirements: Collider (any), Rigidbody, Prompt and assigned interact action button.

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private string itemId = "TrashyTrash";             // item ID so the trash accepts this
        [SerializeField] private GameObject promptIcon;                     // assign the prompt u want in the inspector >:|
        [SerializeField] private bool disableCollidersWhileCarried = true;  // disable colliders so nothing gets messed up :)
        [SerializeField] private float dropCollisionIgnoreTime = 0.5f;      // this stops the collision from immeditly activating, no bugs !

        // this here will fix a very funny bug, dont worry about it lol
        private Vector3 _originalScale;

        public string ItemId => itemId;

        private Collider[] _colliders;
        private Rigidbody _rb;

        private void Awake()
        {
            _colliders = GetComponentsInChildren<Collider>();
            _rb = GetComponent<Rigidbody>();
            _originalScale = transform.localScale;  // stores the size so it wont bug on drop
        }
        public void ShowPrompt() => promptIcon?.SetActive(true);
        public void HidePrompt() => promptIcon?.SetActive(false);

        // this down here stops the player from carrying two things at once :)
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
        public void Drop(Collider playerCollider)
        {
            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.useGravity = true;
            }

            if (disableCollidersWhileCarried)
                foreach (var c in _colliders)
                    c.enabled = true;

            transform.localScale = _originalScale;  // reset object size 

            if (playerCollider != null)
                StartCoroutine(IgnorePlayerCollision(playerCollider));
        }

        private IEnumerator IgnorePlayerCollision(Collider playerCollider)
        {
            // this ienumerator is to stop collision from overlapping with player ! :)
            foreach (var c in _colliders)
                Physics.IgnoreCollision(c, playerCollider, true);

            yield return new WaitForSeconds(dropCollisionIgnoreTime);

            foreach (var c in _colliders)
                Physics.IgnoreCollision(c, playerCollider, false);
        }
    }
}
