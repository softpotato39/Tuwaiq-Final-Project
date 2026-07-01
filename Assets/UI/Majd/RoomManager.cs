using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public RoomDetector currentRoom;

    void Awake()
{
    Instance = this;
    currentRoom = null;
}

    public void SetRoom(RoomDetector room)
    {
        currentRoom = room;
    }

    public void ClearRoom(RoomDetector room)
    {
        if (currentRoom == room)
            currentRoom = null;
    }

    public bool IsInRoom()
    {
        return currentRoom != null;
    }
}