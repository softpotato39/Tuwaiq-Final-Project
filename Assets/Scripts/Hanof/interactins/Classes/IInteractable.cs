using UnityEngine;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//  nothing ! it's just a class we need :)  //
//                                          //
//////////////////////////////////////////////

// script's purpose: create a class to be used in other scripts :O

// script's requirements: none just have in in ur project >:)

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    public interface IInteractable
    {
        void ShowPrompt();                             // show prompt when player raycast comes in
        void HidePrompt();                             // hide prompt when player raycast goes away
        bool CanInteract(PlayerInteractor interactor); // for trash basically, so player cant burn air lol
        void Interact(PlayerInteractor interactor);    // yippy
    }
}
