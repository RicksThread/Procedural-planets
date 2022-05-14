using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UtilityPack.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private CamController camController;
    [SerializeField] private MoveController moveController;
    private PlayerInputs playerInputActions;
    
    private InputControllerAxis2D moveAxisControllerInput;
    private InputControllerAxis2D camMoveAxisControllerInput;
    private InputControllerButton jumpControllerInput;
    private InputControllerButton SprintControllerInput;

    private void Awake() {

        Cursor.lockState = CursorLockMode.Locked;

        //initializing the instance that holds the input events
        playerInputActions = new PlayerInputs();
        
        //enabling the input actions
        playerInputActions.Enable();
        playerInputActions.BaseMovement.Enable();

        //initializing the helper class for the input
        moveAxisControllerInput = new InputControllerAxis2D(playerInputActions.BaseMovement.Move2D);
        camMoveAxisControllerInput = new InputControllerAxis2D(playerInputActions.BaseMovement.CamDelta);
        jumpControllerInput = new InputControllerButton(playerInputActions.BaseMovement.Jump);
        SprintControllerInput = new InputControllerButton(playerInputActions.BaseMovement.Use);
    }

    private void Update()
    {
        if (moveController != null)
        {
            if (jumpControllerInput.GetInputStart())
            {
                moveController.Jump();
            }

            if (SprintControllerInput.GetInputStart())
            {
                moveController.SetSprintState(true);
            }
            else if (SprintControllerInput.GetInputEnd())
            {
                moveController.SetSprintState(false);
            }
            
            moveController.InputMove(moveAxisControllerInput.GetAxis());
        }

        camController.InputRotate(camMoveAxisControllerInput.GetAxis() * 0.1f);
    }

    private void FixedUpdate()
    {
        moveController.InputMove(moveAxisControllerInput.GetAxis());    
    }

    private void OnDestroy() {
        moveAxisControllerInput.Dispose();
        camMoveAxisControllerInput.Dispose();
        jumpControllerInput.Dispose();
    }
}
