using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ambiencefader : MonoBehaviour
{
    public float fadeTime = 1.0f;
    [Range(0f, 1f)] public float targetVolume = 0.5f;   // the volume u want
    private AudioSource roomyambience;
    private Coroutine fader;

    void Awake()
    {
        roomyambience = GetComponent<AudioSource>();
        roomyambience.volume = 0f; // Start silent
        roomyambience.Play();      // Start playing but muted
    }

    void OnTriggerEnter(Collider other)
    {
        // once the PLAYER !! enters the trigger the ambience fades in >:)
        if (other.CompareTag("Player"))
        {
            if (fader != null) StopCoroutine(fader);
            fader = StartCoroutine(FadeBro(targetVolume));
        }
    }

    void OnTriggerExit(Collider other)
    {
        // once the PLAYER !! leaves the trigger the ambience fades away :(
        if (other.CompareTag("Player"))
        {
            if (fader != null) StopCoroutine(fader);
            fader = StartCoroutine(FadeBro(0f));
        }
    }

    private IEnumerator FadeBro(float targetVolume)
    {
        float startVolume = roomyambience.volume;
        float timeElapsed = 0f;

        while (timeElapsed < fadeTime)
        {
            timeElapsed += Time.deltaTime;
            roomyambience.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / fadeTime);
            yield return null;
        }

        roomyambience.volume = targetVolume;
    }
}
