using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private GameObject room;

    [Header("Points")]
    [SerializeField] private Transform doorStart;
    [SerializeField] private Transform doorStop;
    [SerializeField] private Transform doorEnd;
    [SerializeField] private Transform doorHidden;

    [Header("Movement")]
    [SerializeField] private float speed = 3f;

    private bool movingToStop;
    private bool movingToEnd;

    public bool IsActive { get; private set; }

    private void Start()
    {
        transform.position = doorHidden.position;

        if (room != null)
            room.SetActive(false);
    }

    private void Update()
    {
        if (movingToStop)
        {
            transform.position = Vector3.MoveTowards(transform.position, doorStop.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, doorStop.position) < 0.01f)
            {
                movingToStop = false;
                IsActive = true;

                if (room != null)
                    room.SetActive(true);
            }
        }

        if (movingToEnd)
        {
            transform.position = Vector3.MoveTowards(transform.position, doorEnd.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, doorEnd.position) < 0.01f)
            {
                movingToEnd = false;
                IsActive = false;

                if (room != null)
                    room.SetActive(false);

                transform.position = doorHidden.position;
            }
        }
    }

    public void SpawnDoor()
    {
        transform.position = doorStart.position;
        movingToStop = true;
    }

    public void FinishDoor()
    {
        movingToEnd = true;
    }
}