using System.Collections;
using UnityEngine;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//               the Trash !                //
//                                          //
//////////////////////////////////////////////

// script's purpose: let's the player walk up to "trash/disposal" and get a prompt,
//                   interact with it to dispose of the object they are carrying, which
//                   plays an animation and destroys the object ! :)

// script's requirements: Collider (any), Prompt and assigned interact action button.

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
   public class TrashBin : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject promptIcon;             // assign the prompt u want in the inspector :)
        [SerializeField] private string acceptedItemId = "";        //item ID so it only accepts certain objects

        [Header("Feedback")]
        [SerializeField] private Animator binAnimator;              // assign the animated door in the inspector
        [SerializeField] private string disposeTrigger = "Dispose"; // animator trigger call it Dispose :)) same spelling
        [SerializeField] private AudioSource audioSource;       
        [SerializeField] private AudioClip disposeClip;

        public void ShowPrompt() => promptIcon?.SetActive(true);
        public void HidePrompt() => promptIcon?.SetActive(false);

        public bool CanInteract(PlayerInteractor interactor)
        {
            if (!interactor.IsCarryingItem) return false;
            if (string.IsNullOrEmpty(acceptedItemId)) return true;
            return interactor.CurrentItem.ItemId == acceptedItemId;
        }

        //public void Interact(PlayerInteractor interactor)
        //{
        //    if (!CanInteract(interactor)) return;

        //    if (binAnimator != null && !string.IsNullOrEmpty(disposeTrigger))
        //        binAnimator.SetTrigger(disposeTrigger);

        //    if (audioSource != null && disposeClip != null)
        //        audioSource.PlayOneShot(disposeClip);

        //    StartCoroutine(DisposeTime(5f));

        //    PickupItem item = interactor.CurrentItem;
        //    interactor.ClearCarriedItem();
        //    Destroy(item.gameObject);

        //}
        //private IEnumerator DisposeTime(float delay)
        //{
        //    yield return new WaitForSeconds(5f);
        //}

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;

            PickupItem item = interactor.CurrentItem;
            interactor.ClearCarriedItem();

            if (audioSource != null && disposeClip != null)
                audioSource.PlayOneShot(disposeClip);

            if (binAnimator != null && !string.IsNullOrEmpty(disposeTrigger))
            {
                binAnimator.SetTrigger(disposeTrigger);
                StartCoroutine(DestroyAfterAnimation(item));
            }
            else
            {
                Destroy(item.gameObject);
            }
        }

        private IEnumerator DestroyAfterAnimation(PickupItem item)
        {
            // this lets the animation finish before destroying the object :)
            yield return null;

            float animationLength = binAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);

            if (item != null)
                Destroy(item.gameObject);
        }

    }
}
