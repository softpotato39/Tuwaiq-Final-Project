using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene switching

public class voidtrigger : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            SceneManager.LoadScene("AdTime");
        }
    }
}
