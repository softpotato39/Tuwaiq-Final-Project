using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class FactoryTrack : MonoBehaviour
{
    public static FactoryTrack Instance { get; private set; }

    [SerializeField] private List<DoorEntry> allDoors;
    [SerializeField] private int maxFails = 3;

    [SerializeField] private PlayableDirector winDirector;       
    [SerializeField] private PlayableDirector gameOverDirector; 
    [SerializeField] private string winSceneName;
    [SerializeField] private string gameOverSceneName;

    private List<DoorEntry> remainingDoors;
    private DoorEntry currentDoor;
    private int failCount = 0;
    private int successCount = 0;

    public LineState State { get; private set; } = LineState.Idle;
    public event Action<LineState> OnStateChanged;

    void Awake()
    {
        Instance = this;
        remainingDoors = new List<DoorEntry>(allDoors);
    }

    void SetState(LineState next)
    {
        State = next;
        OnStateChanged?.Invoke(next);
    }

    public void CallDoor()
    {
        if (State != LineState.Idle || remainingDoors.Count == 0) return;

        int index = UnityEngine.Random.Range(0, remainingDoors.Count);
        currentDoor = remainingDoors[index];
        remainingDoors.RemoveAt(index);

        SetState(LineState.DoorArriving);
        PlayDoorTimeline(currentDoor.ArrivalAndOpenTimeline, OnArrivalAndOpenComplete);
    }

    void OnArrivalAndOpenComplete()
    {
        currentDoor.RoomRoot.SetActive(true);
        currentDoor.RoomClearChecker.Initialize();
        SetState(LineState.RoomActive);
    }

    public void SendDoorAway()
    {
        if (State != LineState.RoomActive) return;
        SetState(LineState.DoorClosing);
        PlayDoorTimeline(currentDoor.CloseTimeline, OnCloseComplete);
    }

    void OnCloseComplete()
    {
        currentDoor.RoomRoot.SetActive(false);
        SetState(LineState.Assessing);
        RunAssessment();
    }

    void RunAssessment()
    {
        bool cleared = currentDoor.RoomClearChecker.IsRoomCleared();
        SetState(LineState.ResultPlaying);

        if (cleared)
        {
            successCount++;
            bool allDoorsUsed = remainingDoors.Count == 0;

            if (allDoorsUsed && successCount == allDoors.Count)
            {
                PlayGlobalTimeline(winDirector, () => SceneManager.LoadScene(winSceneName));
                return;
            }
            PlayDoorTimeline(currentDoor.SuccessTimeline, ResetToIdle);
        }
        else
        {
            failCount++; // never reset

            if (failCount >= maxFails)
            {
                PlayGlobalTimeline(gameOverDirector, () => SceneManager.LoadScene(gameOverSceneName));
                return;
            }
            PlayDoorTimeline(currentDoor.FailTimeline, ResetToIdle);
        }
    }

    void PlayDoorTimeline(TimelineAsset asset, Action onComplete)
    {
        var director = currentDoor.Director;
        void Handler(PlayableDirector d) { director.stopped -= Handler; onComplete?.Invoke(); }
        director.stopped += Handler;
        director.playableAsset = asset;
        director.Play();
    }

    void PlayGlobalTimeline(PlayableDirector director, Action onComplete)
    {
        void Handler(PlayableDirector d) { director.stopped -= Handler; onComplete?.Invoke(); }
        director.stopped += Handler;
        director.Play();
    }

    void ResetToIdle()
    {
        currentDoor = null;
        SetState(LineState.Idle);
    }
}
