using UnityEngine;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//       World Tool Pickup Mesh! :D         //
//                                          //
//////////////////////////////////////////////

// script's purpose: the item in the game world the player unlocks the tool from, the player
//                   raycast sees it and shows a prompt to pick it up, 
//                   c

// script's requirements: Collider (any), Rigidbody, Prompt, interact button, sound, tool mesh, controls hint.

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    public class ToolKitPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject promptIcon;             // the icon in the ui that pops up
        [SerializeField] private AudioSource pickupSound;           // the that plays once the thingie is destroyed
        [SerializeField] private ToolDefinition[] toolsToUnlock;    // in the inspector u assign the tools u want the player to get here
        [SerializeField] private GameObject ctrlHint;               // this is so we can display control hints once tools are unlocked ! :3
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
