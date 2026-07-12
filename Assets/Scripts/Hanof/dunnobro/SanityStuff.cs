using UnityEngine;
using System.Collections;
//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//         Box Collider Trigger !           //
//                                          //
//////////////////////////////////////////////

// script's purpose: once a player enters the collider they get a prompt 
//                   introducing sanity !

// script's requirements: big box collider on all rooms since it activates once :)
public class SanityStuff : MonoBehaviour
{
    [SerializeField] private GameObject sanityPromptUI;
    [SerializeField] private GameObject tipUI;
    [SerializeField] private float tipDuration = 5f;

    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;

        if (sanityPromptUI != null) sanityPromptUI.SetActive(true);
        if (tipUI != null) tipUI.SetActive(true);

        StartCoroutine(CleanupRoutine());
    }

    private IEnumerator CleanupRoutine()
    {
        yield return new WaitForSeconds(tipDuration);

        if (tipUI != null) Destroy(tipUI);

        // disable the collider so it can never trigger again
        GetComponent<Collider>().enabled = false;
    }
}