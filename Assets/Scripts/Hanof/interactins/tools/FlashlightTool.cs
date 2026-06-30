using UnityEngine;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//       The Flashlight Prefab ! :D         //
//                                          //
//////////////////////////////////////////////

// script's purpose: this will enable the player to "use"
//                   and show UI prompts. can be used for interact or hold to
//                   interact or hold to interact. also tracks held items !

// script's requirements: Flashlight Mesh, Spot Light, click sound AND player input map "Use"

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    public class FlashlightTool : MonoBehaviour, IUsableTool
    {
        [SerializeField] private Light beam;
        [SerializeField] private AudioSource clickSound;
        [SerializeField] private AudioClip clickClip;

        private void Awake()
        {
            if (beam != null) beam.enabled = false; // starts the flashlight turned off wooo
        }

        public void Use()
        {
            // on use it activates/diactivates the spot light and plays the sound :3
            if (beam != null) beam.enabled = !beam.enabled;
            if (clickSound != null && clickClip != null) clickSound.PlayOneShot(clickClip);
        }
    }
}
