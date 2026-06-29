using UnityEngine;
using UnityEngine.Events;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//     Broken thing u want to fix ! :)      //
//                                          //
//////////////////////////////////////////////

// script's purpose: let's the player walk up to "trash/disposal" and get a prompt,
//                   interact with it to dispose of the object they are carrying, which
//                   plays an animation and destroys the object ! :)

// script's requirements: Collider (any), Prompt and assigned, Hold interact action button,
//                        a broken mesh and a fixed mesh. Animation and sound are optional :)

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    public class FixMe : MonoBehaviour, IHoldInteractable
    {
        [Header("Prompt")]
        [SerializeField] private GameObject promptUI;

        [Header("Hold")]
        [SerializeField] private float holdDuration = 2f;

        [Header("Mesh Swap")]
        [SerializeField] private GameObject brokenFing;    // broken mesh ! - active once the game starts 
        [SerializeField] private GameObject fixedFing;     // the fixed mesh after player holds to fix >:)

        [Header("Animator (optional)")]
        [SerializeField] private Animator animator;
        [SerializeField] private string completeTrigger = "Transform";

        [Header("Audio (optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip completeClip;

        [Header("UI Events (optional)")]
        public UnityEvent<float> OnProgress;
        public UnityEvent OnCompleted;

        private bool _isDone;
        public float HoldDuration => holdDuration;

        public void ShowPrompt() => promptUI?.SetActive(!_isDone);
        public void HidePrompt() => promptUI?.SetActive(false);
        public bool CanInteract(PlayerInteractor interactor) => !_isDone;

        // IInteractable needs this to register :,) - the object only responds from the hold calls down there
        public void Interact(PlayerInteractor interactor) { }
        public void OnHoldProgress(float normalizedProgress) => OnProgress?.Invoke(normalizedProgress);
        public void OnHoldComplete(PlayerInteractor interactor)
        {
            _isDone = true;

            // prompt safety check :) this hides it now that the thing is technically
            // fixed but the player raycast is still looking at the object
            HidePrompt();

            if (brokenFing != null) brokenFing.SetActive(false);
            if (fixedFing != null) fixedFing.SetActive(true);

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
