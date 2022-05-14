using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UtilityPack;

public class MoveController : MonoBehaviour
{
    [RequireInterface(typeof(IRigidbodyHandler))]
    [SerializeField] private UnityEngine.Object _body; //the body of the player

    private IRigidbodyHandler body => (IRigidbodyHandler)_body;
    [System.Serializable]
    public class Settings
    {
        [Header("Orientation")]
        public Transform orientation;
        [Header("speeds")]
        public float speedDefault = 56;
        public float sprintSpeedModifier = 1.2f;

        public float defaultDrag = 5.4f;
        [Header("In air")]
        public float InAirSpeedModifier = 0.01f;
        public float InAirDrag = 0.1f;
        [Header("Jump Settings")]
        public float jumpForce = 300f;
        public LayerMask groundMask;
        public float radiusCheck = 0.1f;
        public Transform checkPos;
        
    }

    public Settings settings;

    public event Action hasLanded;
    public event Action hasJumped;

    public bool isSprinting { get; private set; } = false;
    public bool isGrounded { get; private set; } = false;
    public bool takeInput{get; private set; } = true;

    bool isGroundedPrevious = false;
    Vector3 move = Vector3.zero; //store the current player movement input 

    //Get current speed
    float currentSpeed
    {
        get {
            float speed = settings.speedDefault;
            //if the player is in the air
            if (!isGrounded)
            {
                speed *= settings.InAirSpeedModifier;
            }
            //if the player is sprinting
            if (isSprinting) speed *= settings.sprintSpeedModifier;
            return speed;
        }
    }

    private void Awake()
    {
        hasJumped += OnJumped;
        hasLanded += OnLanded;
        isGrounded = CheckIfGrounded();


    }



    private void FixedUpdate()
    {
        //check if the player is grounded and add the force of the movement
        isGrounded = CheckIfGrounded();
        body.AddForce(move, ForceMode.Acceleration);
    }


    public bool CheckIfGrounded()
    {
        bool isGrounded = Physics.CheckSphere(settings.checkPos.position, settings.radiusCheck, settings.groundMask, QueryTriggerInteraction.Ignore);
        //check if the previous value of isGrounded remained the same
        if (isGrounded != isGroundedPrevious)
        {
            //if it is now grounded then call the event
            if (isGrounded)
            {
                hasLanded?.Invoke();
            }
            else
            {
                hasJumped?.Invoke();
            }
        }
        //set the previous state as the current
        isGroundedPrevious = isGrounded;
        return isGrounded;
    }

    void OnJumped()
    {
        body.SetDrag(settings.InAirDrag);
    }

    void OnLanded()
    {
        body.SetDrag(settings.defaultDrag);
    }

    ///<summary>
    ///Get the inputs for the movement
    ///</summary>
    ///<param name="input"> Horizontal and vertical input </param>
    public void InputMove(Vector2 input)
    {
        if (!takeInput) return;
        move = settings.orientation.forward * input.y + settings.orientation.right * input.x;
        move = move.normalized * currentSpeed;
    }

    public void SetSprintState(bool state)
    {
        isSprinting = state;
    }

    public void Jump() 
    {
        //check if it's grounded and it can take inputs
        if (!isGrounded || !takeInput) return;
        
        body.AddForce(settings.orientation.up* settings.jumpForce, ForceMode.Acceleration);
    }

    public void EnableInput(bool state){
        takeInput = state;
    }

    private void OnDestroy()
    {
        //clear the events
        hasLanded = null;
        hasJumped = null;
    }
}
