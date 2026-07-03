using UnityEngine;
using System.Collections;

namespace InteractionSystem
{
    public class RadarTool : MonoBehaviour, IUsableTool
    {
        [SerializeField] private Light pulseLight;
        [SerializeField] private AudioSource beepSound;
        [SerializeField] private AudioClip beepClip;
        [SerializeField] private float pulseDuration = 0.2f;

        private void Awake()
        {
            if (pulseLight != null)
                pulseLight.enabled = false;
        }

        public void Use()
        {
            if (beepSound != null && beepClip != null)
                beepSound.PlayOneShot(beepClip);

            if (pulseLight != null)
                StartCoroutine(Pulse());
        }

        private IEnumerator Pulse()
        {
            pulseLight.enabled = true;
            yield return new WaitForSeconds(pulseDuration);
            pulseLight.enabled = false;
        }
    }
}