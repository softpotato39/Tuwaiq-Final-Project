using UnityEngine;

public class HandleInteraction : MonoBehaviour
{
    [Header("Door System")]
    [SerializeField] private DoorController[] doors;
    [SerializeField] private Animator handleAnimator;

    private DoorController currentDoor;
    private bool playerNear;

    private void Update()
    {
        if (!playerNear) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (currentDoor != null) return;

        if (doors == null || doors.Length == 0) return;

        currentDoor = doors[Random.Range(0, doors.Length)];

        if (handleAnimator != null)
            handleAnimator.SetTrigger("Pull");

        currentDoor.SpawnDoor();
    }

    public void FinishDoor()
    {
        if (currentDoor == null) return;

        currentDoor.FinishDoor();
        currentDoor = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = false;
    }
}