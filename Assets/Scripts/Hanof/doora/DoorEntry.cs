using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//         The Parent of the Door           //
//                                          //
//////////////////////////////////////////////

// script's purpose: a list for us to put all the things we need to activate/deactivate
//                   for each door, and assign the room for it and all that Dx

// script's requirements: door stuff set up :)

// Plain serialized data — configured directly in the FactoryLineManager inspector.
// Not a ScriptableObject: every field here is a scene reference specific to one
// of the 4 fixed doors in this scene, so there's no reuse case an SO would help with.
[System.Serializable]
public class DoorEntry
{
    public string id;

    [Header("Scene References")]
    public GameObject RoomRoot;                  // child object holding the room's contents
    public RoomClearChecker RoomClearChecker;    // lives on RoomRoot
    public PlayableDirector Director;            // lives on the door's own root GameObject

    [Header("Timelines (played by swapping this door's Director.playableAsset)")]
    public TimelineAsset ArrivalAndOpenTimeline;
    public TimelineAsset CloseTimeline;
    public TimelineAsset SuccessTimeline;
    public TimelineAsset FailTimeline;
}
