using UnityEngine;
using UnityEngine.UI;       //we need this for the ring :)

public class HoldActionRing : MonoBehaviour
{
    [SerializeField] private Image ringImage;

    private void Awake()
    {
        ringImage.fillAmount = 0f;
        gameObject.SetActive(false);
    }

    public void OnProgressChanged(float value)
    {
        if (value <= 0f)
        {
            ringImage.fillAmount = 0f;
            gameObject.SetActive(false);        //hides it if its not being used  :<
        }
        else
        {
            gameObject.SetActive(true);         // shows it if its being filled ! :D
            ringImage.fillAmount = value;
        }
    }
}
