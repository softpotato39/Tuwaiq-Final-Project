using UnityEngine;
using UnityEngine.UI;   //we need this for the ring :)

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//         Parent of both circles           //
//                                          //
//////////////////////////////////////////////

// script's purpose: shows the player a visual circle / progress bar filling up as they 
//                   hold to interact :)

// script's requirements: Canvas UI (Screen Space - Overlay) -> [Child] image shape, background -> [Child] actual circle

public class RingUI : MonoBehaviour
{
    [SerializeField] private Image corcle;      // here assign the circle or whatever shape u want to fill up :)

    private void Awake()
    {
        // starts the game with it at zero and not showing up
        corcle.fillAmount = 0f;
        gameObject.SetActive(false);
    }

    public void OnProgressChanged(float value)
    {
        if (value <= 0f)
        {
            corcle.fillAmount = 0f;
            gameObject.SetActive(false);        //hides it if its not being used  :<
        }
        else
        {
            gameObject.SetActive(true);         // shows it if its being filled ! :D
            corcle.fillAmount = value;
        }
    }
}
