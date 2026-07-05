using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//              the Grabber !               //
//                                          //
//////////////////////////////////////////////

// script's purpose: let's the player walk up to "trash/disposal" and get a prompt,
//                   interact with it to dispose of the object they are carrying, which
//                   plays an animation and destroys the object ! :)

// script's requirements: Collider (any), Prompt and assigned interact action button.

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
   public class Exporter : MonoBehaviour, IInteractable
    {
        [Header("Prompt")]
        [SerializeField] private GameObject promptIcon;

        [Header("Timeline")]
        [SerializeField] private PlayableDirector timeline;
        [Tooltip("If true, the grabber can only be used once.")]
        [SerializeField] private bool oneShot = true;

        private bool _hasPlayed = false;

        public void ShowPrompt() => promptIcon?.SetActive(true);

        public void HidePrompt() => promptIcon?.SetActive(false);

        public bool CanInteract(PlayerInteractor interactor)
        {
            if (oneShot && _hasPlayed) return false;
            return true;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;

            _hasPlayed = true;
            HidePrompt();

            if (timeline != null)
                timeline.Play();
        }
    }
}
