using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//         Box Collider on Room !           //
//                                          //
//////////////////////////////////////////////

// script's purpose: uses the collider to check if there are any anomalies inside the
//                   room !, so we know if the room is fully clear or not.

// script's requirements: big box collider covering the room
public class RoomClearChecker : MonoBehaviour
{
    [SerializeField] private Collider roomBounds;

    [SerializeField] private string tagForBaddies = "anomalies";

    private List<Transform> tracked = new List<Transform>();
    public void Initialize()
    {
      // this is so we only track objects inside this room !
        tracked.Clear();
        foreach (var go in GameObject.FindGameObjectsWithTag(tagForBaddies))
            tracked.Add(go.transform);
    }

    public bool IsRoomCleared()
    {
        foreach (var t in tracked)
        {
            if (t == null) continue;                        // if objects are destroyed = removed
            if (!t.gameObject.activeInHierarchy) continue;  // if objects are deactivated = removed
            if (roomBounds.bounds.Contains(t.position))
                return false;                               // still physically in the room
        }
        return true;
    }
}
