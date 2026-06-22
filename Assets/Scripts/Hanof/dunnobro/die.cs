using UnityEngine;

public class die : MonoBehaviour
{

    public float life = 1f;

    void OnEnable()
    {
       Destroy(gameObject,life);
    }

}
