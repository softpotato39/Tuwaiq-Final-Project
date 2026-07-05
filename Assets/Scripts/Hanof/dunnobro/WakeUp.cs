using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WakeUp : MonoBehaviour
{
    [Header("Vision Fade")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float blinkFadeDuration = 0.6f;
    [SerializeField] private float blinkHoldDuration = 0.3f;
    [SerializeField] private float finalWakeFadeDuration = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource snoringSource;

    [Header("Player")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private MonoBehaviour[] scriptsToEnableOnWake; // movement, look, etc.

    private void Start()
    {
        SetPlayerControlEnabled(false);
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        fadeOverlay.alpha = 0f;

        for (int i = 0; i < blinkCount; i++)
        {
            yield return Fade(fadeOverlay, 0f, 1f, blinkFadeDuration);
            yield return new WaitForSeconds(blinkHoldDuration);
            yield return Fade(fadeOverlay, 1f, 0f, blinkFadeDuration);
            yield return new WaitForSeconds(blinkHoldDuration);
        }

        yield return Fade(fadeOverlay, 0f, 1f, blinkFadeDuration);

        float t = 0f;
        float startVolume = snoringSource != null ? snoringSource.volume : 0f;
        while (t < finalWakeFadeDuration)
        {
            t += Time.deltaTime;
            float p = t / finalWakeFadeDuration;
            fadeOverlay.alpha = Mathf.Lerp(1f, 0f, p);
            if (snoringSource != null)
                snoringSource.volume = Mathf.Lerp(startVolume, 0f, p);
            yield return null;
        }

        fadeOverlay.alpha = 0f;
        if (snoringSource != null)
        {
            snoringSource.volume = 0f;
            snoringSource.Stop();
        }

        SetPlayerControlEnabled(true);
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (playerInput != null) playerInput.enabled = enabled;
        foreach (var script in scriptsToEnableOnWake)
            if (script != null) script.enabled = enabled;
    }
}
