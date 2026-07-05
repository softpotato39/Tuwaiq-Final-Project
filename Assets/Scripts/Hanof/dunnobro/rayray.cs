using System.Collections.Generic;
using UnityEngine;

public class rayray : MonoBehaviour
{
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private LayerMask raycastLayerMask = ~0; // "Everything" minus your Wall layer
    [SerializeField] private float shiftForce = 2f;

    [Header("Sound")]
    [SerializeField] private AudioClip shooSound;
    [SerializeField] private float shooSoundVolume = 1f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 15f;

    private readonly HashSet<Collider> alreadyShifted = new HashSet<Collider>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 15);
    }

    void Update()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, raycastDistance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Shoo"))
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null) rb.AddForce(transform.up * shiftForce, ForceMode.Impulse);
                break;
            }
        }
    }

    private void PlaySpatialSoundOn(Transform parent)
    {
        if (shooSound == null) return;

        GameObject soundObj = new GameObject("ShooSound");
        soundObj.transform.SetParent(parent);
        soundObj.transform.localPosition = Vector3.zero;

        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = shooSound;
        source.volume = shooSoundVolume;
        source.spatialBlend = 1f; // fully 3D
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.Play();

        Destroy(soundObj, shooSound.length);
    }
}
