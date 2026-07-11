using UnityEngine;
using UnityEngine.UI;
using InteractionSystem;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//              the handle !                //
//                                          //
//////////////////////////////////////////////

// script's purpose: let's the player walk up to handle by the doors and get a prompt,
//                   interact with it to call/send away doors, which plays a timeline
//                   based on what the player did !

// script's requirements: Collider (any), Prompt with call and send textures, and assigned interact action button.

public class PoppyHandle : MonoBehaviour, IInteractable
{
    [SerializeField] private RawImage promptImage;              // assign the the Raw Image from your Canvas
    [SerializeField] private Texture callPromptTexture;         // the CALL DOOR texture !
    [SerializeField] private Texture sendAwayPromptTexture;     // the SEND AWAY texture !
    [SerializeField] private Animator handleAnimator;           // animator for handle

    void OnEnable() => FactoryTrack.Instance.OnStateChanged += HandleStateChanged;
    void OnDisable() => FactoryTrack.Instance.OnStateChanged -= HandleStateChanged;

    void HandleStateChanged(LineState state)
    {   
      // this here is where the code checks if there's a door or not ! so it can show the right prompt i want :3
        switch (state)
        {
            case LineState.Idle: promptImage.texture = callPromptTexture; break;
            case LineState.RoomActive: promptImage.texture = sendAwayPromptTexture; break;
        }
    }

    public void ShowPrompt() => promptImage.gameObject.SetActive(true);
    public void HidePrompt() => promptImage.gameObject.SetActive(false);

    public bool CanInteract(PlayerInteractor interactor)
    {
        var state = FactoryTrack.Instance.State;
        return state == LineState.Idle || state == LineState.RoomActive;
    }

    public void Interact(PlayerInteractor interactor)
    {
        handleAnimator.SetTrigger("Pull");  // update animator trigger :3

        var state = FactoryTrack.Instance.State;
        if (state == LineState.Idle) FactoryTrack.Instance.CallDoor();
        else if (state == LineState.RoomActive) FactoryTrack.Instance.SendDoorAway();
    }
}