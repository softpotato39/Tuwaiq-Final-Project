using UnityEngine;

namespace InteractionSystem
{
    /// <summary>
    /// Implement on anything the player can interact with via a quick press of Interact
    /// (pickups, the trash bin, terminals, doors, levers, etc).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Show this object's own prompt visual (a world-space icon/GameObject you own and
        /// reference directly - PlayerInteractor doesn't know or care what it looks like).
        /// Called once when this becomes the look-at target and CanInteract is true.
        /// </summary>
        void ShowPrompt();

        /// <summary>
        /// Hide the prompt visual. Called when this stops being the look-at target. Also call
        /// this yourself if your own internal state changes the prompt's validity without the
        /// look-at target changing (e.g. a hold-interactable finishing while still being looked at).
        /// </summary>
        void HidePrompt();

        /// <summary>Whether this object can currently be interacted with (e.g. trash only works while carrying an item).</summary>
        bool CanInteract(PlayerInteractor interactor);

        /// <summary>Called once on a quick press of Interact. For hold-based objects this can be left empty - see IHoldInteractable.</summary>
        void Interact(PlayerInteractor interactor);
    }
}
