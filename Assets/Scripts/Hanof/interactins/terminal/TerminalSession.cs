using Unity.Cinemachine;
using UnityEngine;

namespace InteractionSystem
{
    /// <summary>
    /// Drop this on (or near) each individual terminal. Holds everything specific to that
    /// one terminal - its Cinemachine camera, screen visuals, and power-on sound - so
    /// TerminalController itself stays generic and works with as many terminals as you place.
    /// </summary>
    public class TerminalSession : MonoBehaviour
    {
        [Header("Camera")]
        [Tooltip("Dedicated CinemachineCamera framing this terminal's screen. Keep its priority low/default until activated.")]
        [SerializeField] private CinemachineCamera terminalCamera;

        [Header("Screen")]
        [SerializeField] private GameObject screenOnVisual;
        [SerializeField] private GameObject screenOffVisual;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip powerOnClip;

        public CinemachineCamera Camera => terminalCamera;

        public void PowerOn()
        {
            if (screenOnVisual != null) screenOnVisual.SetActive(true);
            if (screenOffVisual != null) screenOffVisual.SetActive(false);
            if (audioSource != null && powerOnClip != null) audioSource.PlayOneShot(powerOnClip);
        }

        public void PowerOff()
        {
            if (screenOnVisual != null) screenOnVisual.SetActive(false);
            if (screenOffVisual != null) screenOffVisual.SetActive(true);
        }
    }
}
