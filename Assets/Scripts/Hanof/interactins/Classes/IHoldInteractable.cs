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
    public interface IHoldInteractable : IInteractable
    {
        float HoldDuration { get; }                         // how long u have to hold to accomplish the task
        void OnHoldProgress(float normalizedProgress);      // progress the counter from 0 to 1 :D
        void OnHoldComplete(PlayerInteractor interactor);   // completion yippy
        void OnHoldCancelled();                             // when the player looks away or lets go of hold we make them restart >:)
    }
}
