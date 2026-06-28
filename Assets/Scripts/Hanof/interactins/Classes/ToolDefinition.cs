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
    // this makes it so we can create a tool defenition from the Create window when we right click ! how cool
    [CreateAssetMenu(menuName = "Interaction System/Tool Definition", fileName = "NewTool")]
    public class ToolDefinition : ScriptableObject
    {
        public string toolName;     // name of the tool player will unlock and u will assign in the tool pickup script
        public Sprite icon;         // tool icon (this is for in case we decide to put an inventory system i'm not sure yet)
        public GameObject toolPrefab; // the tool and all its stuff that the player will aquip and use ! :D
    }
}
