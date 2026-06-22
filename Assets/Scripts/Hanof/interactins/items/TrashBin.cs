using UnityEngine;

namespace InteractionSystem
{
    /// <summary>
    /// Disposal point for PickupItems. Quick-press Interact while carrying an (optionally
    /// matching) item to destroy it, trigger the bin's "dispose" animation, and play a sound.
    /// </summary>
    public class TrashBin : MonoBehaviour, IInteractable
    {
        [Tooltip("World-space icon/GameObject shown while this is the look-at target and CanInteract is true.")]
        [SerializeField] private GameObject promptIcon;
        [Tooltip("Leave blank to accept any carried item. Set to match PickupItem.ItemId to only accept specific trash.")]
        [SerializeField] private string acceptedItemId = "";

        [Header("Feedback")]
        [SerializeField] private Animator binAnimator;
        [SerializeField] private string disposeTrigger = "Dispose";
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

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;

            PickupItem item = interactor.CurrentItem;
            interactor.ClearCarriedItem();
            Destroy(item.gameObject);

            if (binAnimator != null && !string.IsNullOrEmpty(disposeTrigger))
                binAnimator.SetTrigger(disposeTrigger);

            if (audioSource != null && disposeClip != null)
                audioSource.PlayOneShot(disposeClip);
        }
    }
}
