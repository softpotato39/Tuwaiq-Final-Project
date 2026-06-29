using Unity.Cinemachine;
using UnityEngine;

//////////////////////////////////////////////
//                                          //
//       This script shall live on:         //
//            The Terminal :O               //
//                                          //
//////////////////////////////////////////////

// script's purpose: allows the player to turn on and off the screen for
//                   the terminal once interacted with from the script:
//                   TerminalInteractable.

// script's requirements: Collider (any), Prompt, interact button, sounds on/off, screen on/of mesh and Cinemachine cam !

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    public class TerminalSession : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private CinemachineCamera terminalCamera;      // assign the cam u want in the inspector :)

        [Header("Screen")]
        [SerializeField] private GameObject screenOnVisual;             // what shows up once you turn ON the terminal
        [SerializeField] private GameObject screenOffVisual;            // what shows up once you turn OFF the terminal

        [Header("Audio")]
        [SerializeField] private AudioSource onAudioObj;               // sounds for on
        [SerializeField] private AudioClip powerOnClip;                //
        [SerializeField] private AudioSource offAudioObj;              // sounds for off
        [SerializeField] private AudioClip powerOffClip;               //
        public CinemachineCamera Camera => terminalCamera;

        public void PowerOn()
        {
            if (screenOnVisual != null) screenOnVisual.SetActive(true);
            if (screenOffVisual != null) screenOffVisual.SetActive(false);
            if (onAudioObj != null && powerOnClip != null) onAudioObj.PlayOneShot(powerOnClip);
        }

        public void PowerOff()
        {
            if (screenOnVisual != null) screenOnVisual.SetActive(false);
            if (screenOffVisual != null) screenOffVisual.SetActive(true);
            if (offAudioObj != null && powerOnClip != null) offAudioObj.PlayOneShot(powerOnClip);
        }
    }
}
