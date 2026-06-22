using UnityEngine;

namespace InteractionSystem
{
    /// <summary>
    /// Example IUsableTool. Put this on the flashlight prefab's root (the same prefab
    /// referenced by its ToolDefinition). Pressing ToolController's "use" action while the
    /// flashlight is equipped calls Use(), which flips the beam on/off and plays a click.
    /// </summary>
    public class FlashlightTool : MonoBehaviour, IUsableTool
    {
        [SerializeField] private Light beam;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleClip;

        private void Awake()
        {
            if (beam != null) beam.enabled = false; // starts off
        }

        public void Use()
        {
            if (beam != null) beam.enabled = !beam.enabled;
            if (audioSource != null && toggleClip != null) audioSource.PlayOneShot(toggleClip);
        }
    }
}
