using UnityEngine;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//         Box Collider Trigger !           //
//                                          //
//////////////////////////////////////////////

// script's purpose: once a player enters the collider in elevator ride down 
//                   the projector room is gone :)

// script's requirements: box collider trigger on elevator
public class ProjectorStuff : MonoBehaviour
{
    [SerializeField] private GameObject byeByeProjector;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (byeByeProjector != null)
            Destroy(byeByeProjector);

        GetComponent<Collider>().enabled = false;
    }
}
