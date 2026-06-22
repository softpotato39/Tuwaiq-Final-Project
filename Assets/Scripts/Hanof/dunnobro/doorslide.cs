using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class doorslide : MonoBehaviour
{
    public GameObject doora;
    private void OnTriggerEnter(Collider hello)
    {
        if (hello.gameObject.tag == "Player")
        {
            Debug.Log("opensesame works");
            doora.GetComponent<Animator>().Play("opensesame");
        }
    }

    private void OnTriggerExit(Collider hello)
    {
        if (hello.gameObject.tag == "Player")
        {
            Debug.Log("close works");
            doora.GetComponent<Animator>().Play("closesesame");
        }
    }
}
