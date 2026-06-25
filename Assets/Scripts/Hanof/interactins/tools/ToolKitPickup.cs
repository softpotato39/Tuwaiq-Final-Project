using UnityEngine;

namespace InteractionSystem
{

    // this script stays on: the objects the player goes to pick up !
    // basically a kit u chose what prefab tools to unlock
    public class ToolKitPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject promptIcon;    // the icon in the ui that pops up
        [SerializeField] private AudioSource pickupSound;   // the that plays once the thingie is destroyed
        [SerializeField] private ToolDefinition[] toolsToUnlock;    // in the inspector u assign the tools u want the player to get here
        [SerializeField] private GameObject ctrlHint;       // i put this so we can display hints ! :3
        [SerializeField] private bool destroyOnPickup = true;

        public void ShowPrompt() => promptIcon?.SetActive(true);

        public void HidePrompt() => promptIcon?.SetActive(false);

        public bool CanInteract(PlayerInteractor interactor) => true;

        public void Interact(PlayerInteractor interactor)
        {
            ToolController controller = interactor.GetComponent<ToolController>();
            if (controller == null)
            {
                Debug.LogWarning("missing toolcontroller on PLAYER !", this);
                return;
            }

            controller.UnlockTools(toolsToUnlock);

            if (destroyOnPickup)
                promptIcon.SetActive(false);
                ctrlHint.SetActive(true);
                pickupSound.Play();
                Destroy(gameObject);

        }
    }
}
