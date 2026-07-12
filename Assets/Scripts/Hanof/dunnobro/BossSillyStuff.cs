using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//         Box Collider Trigger !           //
//                                          //
//////////////////////////////////////////////

// script's purpose: if the player spends a certain amount of time in the trigger,
//                   certain audio files shall play ;3

// script's requirements: box collider trigger wherever u wanna stand to eavesdrop :3

public class BossSillyStuff : MonoBehaviour
{
    [SerializeField] private AudioSource[] audioSources;
    [SerializeField] private float delayBetweenSounds = 5f;

    private bool _playerInside = false;
    private Coroutine _sequenceRoutine;
    private List<int> _playlist = new List<int>();
    private int _currentIndex = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = true;

        if (_sequenceRoutine == null)
            _sequenceRoutine = StartCoroutine(SequenceRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = false;

        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }
    }

    private IEnumerator SequenceRoutine()
    {
        // build a fresh playlist for the first time
        if (_playlist.Count == 0)
            BuildPlaylist();

        while (true)
        {
            // wait the delay while the player stays inside
            float elapsed = 0f;
            while (elapsed < delayBetweenSounds)
            {
                if (!_playerInside) yield break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // plays next sound in the playlist
            int sourceIndex = _playlist[_currentIndex];
            if (audioSources[sourceIndex] != null)
                audioSources[sourceIndex].Play();

            _currentIndex++;

            // when all my 9 have played, reshuffle the playlist :3
            // so they all play again before any repeats
            if (_currentIndex >= _playlist.Count)
            {
                _currentIndex = 0;
                BuildPlaylist();
            }
        }
    }

    private void BuildPlaylist()
    {
        // scary math for shuffle stuff :)

        _playlist.Clear();
        for (int i = 0; i < audioSources.Length; i++)
            _playlist.Add(i);

        for (int i = _playlist.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = _playlist[i];
            _playlist[i] = _playlist[j];
            _playlist[j] = temp;
        }
    }
}
