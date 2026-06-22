using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class lodwig : MonoBehaviour
{
    public GameObject smoke;
    public GameObject goodie;
    public GameObject baddie;
    public AudioSource poof;

    //THERE ARE TWO WAYS WE CAN DO THIS !
    // 1 - we could have fire cover the objects
    // 2 fire exists without any objects
    // --------- ONLY ONE CAN BE ACTIVE AT A TIME ----------

    // 1 
    private void OnTriggerEnter(Collider hello)
    {
        if (hello.gameObject.tag == "Player")
        {
            Debug.Log("woah");
            baddie.SetActive(false);
            smoke.SetActive(true);
            StartCoroutine(FireAway());
            goodie.SetActive(true);
            poof.Play();
        }
    }

    private void OnTriggerExit(Collider hello)
    {
        if (hello.gameObject.tag == "Player")
        {
            Debug.Log("oooh");
            smoke.SetActive(true);
            StartCoroutine(FireAway());
            goodie.SetActive(false);
            baddie.SetActive(true);
            poof.Play();
        }
    }
    private IEnumerator FireAway()
    {
        yield return new WaitForSeconds(0.3f);
        smoke.SetActive(false);
    }


    // 2
    //private void OnTriggerEnter(Collider hello)
    //{
    //    if (hello.gameObject.tag == "Player")
    //    {
    //        Debug.Log("woah");
    //        StartCoroutine(Innie());
    //    }
    //}
    //private void OnTriggerExit(Collider hello)
    //{
    //    if (hello.gameObject.tag == "Player")
    //    {
    //        Debug.Log("oooh");
    //        StartCoroutine(Outie());
    //    }
    //}
    //private IEnumerator Innie()
    //{
    //    poof.Play();
    //    baddie.SetActive(false);
    //    smoke.SetActive(true);
    //    yield return new WaitForSeconds(0.3f);
    //    smoke.SetActive(false);
    //    yield return new WaitForSeconds(0.2f);
    //    goodie.SetActive(true);
    //}

    //private IEnumerator Outie()
    //{
    //    poof.Play();
    //    goodie.SetActive(false);
    //    smoke.SetActive(true);
    //    yield return new WaitForSeconds(0.3f);
    //    smoke.SetActive(false);
    //    yield return new WaitForSeconds(0.2f);
    //    baddie.SetActive(true);
    //}
}
