using UnityEngine;
using UnityEngine.Playables;

public class BossHere : MonoBehaviour
{
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool onlyTriggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (onlyTriggerOnce && hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        hasTriggered = true;
        timelineDirector.Play();
    }
}
