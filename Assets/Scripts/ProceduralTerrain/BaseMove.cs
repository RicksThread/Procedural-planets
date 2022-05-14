using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UtilityPack.InputSystem;

public class BaseMove : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform moveTransform;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform parentOrientation;
    [SerializeField] private float sensitivity;
    private PlayerInputs playerActionInput;
    private InputControllerAxis2D inputMoveControllerAxis2D;
    private InputControllerAxis2D inputRotateControllerAxis2D;




    private void Start()
    {
        playerActionInput = new PlayerInputs();
        playerActionInput.Enable();
        playerActionInput.BaseMovement.Enable();

        inputMoveControllerAxis2D = new InputControllerAxis2D(playerActionInput.BaseMovement.Move2D);
        inputRotateControllerAxis2D = new InputControllerAxis2D(playerActionInput.BaseMovement.CamDelta);
    }

    private void Update() 
    {
        moveTransform.position += (inputMoveControllerAxis2D.GetAxis().x * orientation.right + inputMoveControllerAxis2D.GetAxis().y * orientation.forward) * Time.deltaTime * speed ;
        orientation.Rotate(Vector3.right * -inputRotateControllerAxis2D.GetAxis().y * sensitivity + Vector3.up* inputRotateControllerAxis2D.GetAxis().x * sensitivity);
    
    }
}
