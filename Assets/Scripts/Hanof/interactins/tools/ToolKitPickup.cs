using UnityEngine;

namespace InteractionSystem
{
    /// <summary>
    /// A "kit" the player picks up once to unlock several ToolDefinitions on their
    /// ToolController, e.g. a janitor kit that unlocks Mop, Sponge and Vacuum.
    /// </summary>
    public class ToolKitPickup : MonoBehaviour, IInteractable
    {
        [Tooltip("World-space icon/GameObject shown while this is the look-at target.")]
        [SerializeField] private GameObject promptIcon;
        [SerializeField] private ToolDefinition[] toolsToUnlock;
        [SerializeField] private bool destroyOnPickup = true;

        public void ShowPrompt() => promptIcon?.SetActive(true);

        public void HidePrompt() => promptIcon?.SetActive(false);

        public bool CanInteract(PlayerInteractor interactor) => true;

        public void Interact(PlayerInteractor interactor)
        {
            ToolController controller = interactor.GetComponent<ToolController>();
            if (controller == null)
            {
                Debug.LogWarning("ToolKitPickup: no ToolController found on the player.", this);
                return;
            }

            controller.UnlockTools(toolsToUnlock);

            if (destroyOnPickup)
                Destroy(gameObject);
        }
    }
}
