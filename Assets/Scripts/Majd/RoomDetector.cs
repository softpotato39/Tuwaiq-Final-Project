using UnityEngine;

public class RoomDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ENTER ROOM");
            RoomManager.Instance.SetRoom(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("EXIT ROOM");
            RoomManager.Instance.ClearRoom(this);
        }
    }
}