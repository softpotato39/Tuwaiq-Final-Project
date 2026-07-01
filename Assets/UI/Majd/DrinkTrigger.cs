using UnityEngine;

public class DrinkTrigger : MonoBehaviour
{
    public SanitySystem sanitySystem;

    private bool playerNear = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            Debug.Log("Press E to drink");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }

    private void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Drank Water");

            sanitySystem.sanity += 30f;
            sanitySystem.sanity = Mathf.Clamp(sanitySystem.sanity, 0f, 100f);

            gameObject.SetActive(false);
        }
    }
}