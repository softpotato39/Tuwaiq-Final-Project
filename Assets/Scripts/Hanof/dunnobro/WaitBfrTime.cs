using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class WaitBfrTime : MonoBehaviour
{
    [Tooltip("The Playable Director component of the timeline you want to play")]
    public PlayableDirector timelineDirector;

    [Tooltip("The delay in seconds before the timeline starts")]
    public float delayInSeconds = 1.10f;

    void Start()
    {
        if (timelineDirector != null)
        {
            StartCoroutine(WaitAndPlayTimeline());
        }
    }

    private IEnumerator WaitAndPlayTimeline()
    {
        // Wait for the specified amount of time
        yield return new WaitForSeconds(delayInSeconds);

        // Play the timeline
        timelineDirector.Play();
    }
}
