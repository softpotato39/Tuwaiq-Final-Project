using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace InteractionSystem
{
    public class RadarScannerTool : MonoBehaviour, IUsableTool
    {
        [Header("Scan")]
        [SerializeField] private float scanRadius = 15f;
        [SerializeField] private float highlightDuration = 3f;
        [SerializeField] private float cooldown = 2f;

        [Header("Wave Effect")]
        [SerializeField] private GameObject scanWavePrefab;

        [Header("Audio")]
        private AudioSource audioSource;
        [SerializeField] private AudioClip scanClip;

        private bool _onCooldown = false;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();  // this is to fix the sound bug >:(
        }

        public void Use()
        {
            if (_onCooldown) return;
            StartCoroutine(ScanRoutine());
        }

        private IEnumerator ScanRoutine()
        {
            _onCooldown = true;

            // sound
            if (audioSource != null && scanClip != null)
                audioSource.PlayOneShot(scanClip);

            // spawn wave at player position, unparented so it stays
            // in world space even if the player moves
            if (scanWavePrefab != null)
            {
                GameObject wave = Instantiate(scanWavePrefab, transform.position, Quaternion.identity);
                wave.transform.SetParent(null);
            }

            // find every collider in range, grab AnomalyHighlight from
            // anything tagged "anomaly" and light it up
            Collider[] hits = Physics.OverlapSphere(transform.position, scanRadius);
            List<AnomalyHighlight> found = new List<AnomalyHighlight>();

            foreach (var hit in hits)
            {
                if (hit.CompareTag("anomalies"))
                {
                    AnomalyHighlight highlight = hit.GetComponentInParent<AnomalyHighlight>();
                    if (highlight != null && !found.Contains(highlight))
                    {
                        found.Add(highlight);
                        highlight.ShowHighlight(highlightDuration);
                    }
                }
            }

            yield return new WaitForSeconds(cooldown);
            _onCooldown = false;
        }
    }
}
