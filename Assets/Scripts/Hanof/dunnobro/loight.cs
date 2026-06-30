using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class loight : MonoBehaviour
{
    public GameObject buzz;     // the light
    public AudioSource squeex;  // the sound
    private void OnTriggerEnter(Collider hello)
    {
        if (hello.gameObject.tag == "Player")
        {
            Debug.Log("ewww works");        //degub log lol
            squeex.Play();      //plays the sound we want
            buzz.GetComponent<Animator>().Play("dddddd");   //plays the animation we want
            Destroy(gameObject);    //this destroyes the trigger so it doesnt keep playing :)))
        }
    }
}
