using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

public class VidLoop : MonoBehaviour
{
    [SerializeField] private UnityEvent onFirstLoopComplete;
    private VideoPlayer videoPlayer;
    private bool hasTriggered;

    private void Awake() => videoPlayer = GetComponent<VideoPlayer>();

    private void OnEnable() => videoPlayer.loopPointReached += HandleLoopPointReached;
    private void OnDisable() => videoPlayer.loopPointReached -= HandleLoopPointReached;

    private void HandleLoopPointReached(VideoPlayer vp)
    {
        if (hasTriggered) return;
        hasTriggered = true;
        onFirstLoopComplete?.Invoke();
    }
}
