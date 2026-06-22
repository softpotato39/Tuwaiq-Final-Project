namespace InteractionSystem
{
    /// <summary>
    /// Implement alongside IInteractable on objects that require the player to hold the
    /// Interact button for a duration, e.g. turning an unmade bed into a tidy one.
    /// PlayerInteractor checks for this interface automatically - if present, it drives the
    /// hold timer and calls these instead of a single Interact().
    /// </summary>
    public interface IHoldInteractable : IInteractable
    {
        /// <summary>Seconds the button must be held to complete the interaction.</summary>
        float HoldDuration { get; }

        /// <summary>Called every frame while held, normalizedProgress goes 0 to 1.</summary>
        void OnHoldProgress(float normalizedProgress);

        /// <summary>Called once when the hold reaches HoldDuration.</summary>
        void OnHoldComplete(PlayerInteractor interactor);

        /// <summary>Called if the player releases early or looks away before completion.</summary>
        void OnHoldCancelled();
    }
}
