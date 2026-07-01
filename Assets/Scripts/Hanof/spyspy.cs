using UnityEngine;

public class spyspy : MonoBehaviour
{
    // use serialized field to change stuff in unity editor but keep it secure :)
    [SerializeField] private Transform playaPlace;      //player's position
    [SerializeField] private float spinSpeed = 5.0f;    //speed for rotating
    [SerializeField] private bool onlyUppies = false;    // basically locks the thing thats tracking the player to only look up and down

    void Update()
    {
        if (playaPlace == null) return;

        // get the direction vector from the thingie to the player
        Vector3 direction = playaPlace.position - transform.position;

        if (onlyUppies)
        {
            direction.y = 0; // trust me u need this :) it stops weird tilting
        }

        // only do the rotating if the direction vector is not zero
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly interpolate from current rotation to target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, spinSpeed * Time.deltaTime);
        }
    }
}

