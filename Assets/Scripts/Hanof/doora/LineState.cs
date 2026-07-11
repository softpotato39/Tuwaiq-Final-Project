//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//              Nothing ! :D                //
//                                          //
//////////////////////////////////////////////

// script's purpose: a list to keep count of the things we need to fully
//                   activate the whole door sequence

// script's requirements: nothing ! :)
public enum LineState
{
    Idle,               // handle can be used to call a door
    DoorArriving,       // door arrives timeline playing
    RoomActive,         // YIPPY now the player is free to go in/out and clear the room
    DoorClosing,        // door leaving timeline playing
    Assessing,          // room-clear check running
    ResultPlaying,      // one of 4 timelines plays :) ---> success / fail / win / game over
    GameOverSequence    // game over timeline finished, load death menu
}
