using UnityEngine;
using UnityEngine.Events;

namespace InteractionSystem
{
    /// <summary>
    /// A hold-to-interact object that transitions between two states, e.g. an unmade bed
    /// becoming a tidy one. Swap GameObjects/meshes, drive an Animator, or just listen to
    /// OnCompleted - whichever fits the object.
    /// </summary>
    public class TransfromInteractable : MonoBehaviour, IHoldInteractable
    {
        [Header("Prompt")]
        [Tooltip("World-space icon/GameObject shown while this is the look-at target and not yet completed.")]
        [SerializeField] private GameObject promptUI;

        [Header("Hold")]
        [SerializeField] private float holdDuration = 2f;

        [Header("Visual Swap (optional)")]
        [Tooltip("Active before the interaction completes (e.g. the messy bed mesh).")]
        [SerializeField] private GameObject beforeState;
        [Tooltip("Activated once the hold completes (e.g. the tidy bed mesh).")]
        [SerializeField] private GameObject afterState;

        [Header("Animator (optional)")]
        [SerializeField] private Animator animator;
        [SerializeField] private string completeTrigger = "Transform";

        [Header("Audio (optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip completeClip;

        [Header("Events")]
        [Tooltip("0..1 while held - hook a world-space or screen-space progress bar to this.")]
        public UnityEvent<float> OnProgress;
        public UnityEvent OnCompleted;

        private bool _isDone;

        public float HoldDuration => holdDuration;

        public void ShowPrompt() => promptUI?.SetActive(!_isDone);

        public void HidePrompt() => promptUI?.SetActive(false);

        public bool CanInteract(PlayerInteractor interactor) => !_isDone;

        // Required by IInteractable - this object only ever responds via the hold callbacks below.
        public void Interact(PlayerInteractor interactor) { }

        public void OnHoldProgress(float normalizedProgress) => OnProgress?.Invoke(normalizedProgress);

        public void OnHoldComplete(PlayerInteractor interactor)
        {
            _isDone = true;

            // The player is almost certainly still looking straight at this object when the hold
            // finishes, so PlayerInteractor won't see a target change to trigger HidePrompt() on
            // its own - hide it ourselves the moment we know we're done.
            HidePrompt();

            if (beforeState != null) beforeState.SetActive(false);
            if (afterState != null) afterState.SetActive(true);

            if (animator != null && !string.IsNullOrEmpty(completeTrigger))
                animator.SetTrigger(completeTrigger);

            if (audioSource != null && completeClip != null)
                audioSource.PlayOneShot(completeClip);

            OnProgress?.Invoke(0f);
            OnCompleted?.Invoke();
        }

        public void OnHoldCancelled() => OnProgress?.Invoke(0f);
    }
}
