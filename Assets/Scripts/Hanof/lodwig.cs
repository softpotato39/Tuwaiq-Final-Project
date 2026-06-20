using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class lodwig : MonoBehaviour
{
    public GameObject poof;
    private void OnTriggerEnter(Collider hello)
    {
        if (hello.gameObject.tag == "Player")
        {
            Debug.Log("woah");
            poof.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider hello)
    {
        if (hello.gameObject.tag == "Player")
        {
            Debug.Log("oooh");
            poof.SetActive(true);
        }
    }
}
