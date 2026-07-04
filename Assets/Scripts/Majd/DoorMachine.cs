using UnityEngine;
using InteractionSystem;

public class DoorMachine : MonoBehaviour, IInteractable
{
    [SerializeField] private DoorController[] doors;
    [SerializeField] private Animator handleAnimator;

    private DoorController currentDoor;
    private bool busy;

    public void ShowPrompt() { }
    public void HidePrompt() { }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return !busy;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (busy) return;
        if (doors == null || doors.Length == 0) return;

        busy = true;

        currentDoor = doors[Random.Range(0, doors.Length)];

        if (handleAnimator != null)
            handleAnimator.SetTrigger("Pull");

        if (currentDoor != null)
            currentDoor.SpawnDoor();
    }

    private void Update()
    {
        if (currentDoor == null)
        {
            busy = false;
            return;
        }

        if (!currentDoor.IsActive)
        {
            currentDoor = null;
            busy = false;
        }
    }

    public void FinishCurrentDoor()
    {
        if (currentDoor == null) return;

        currentDoor.FinishDoor();
    }
}