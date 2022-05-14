using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip : PhysicsBody
{
    [Header("Control specs")]
    [SerializeField] float acceleration;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float rotVelocity;


    float mouseX, mouseY, rollInput;
    float horizontal, vertical, foward;

    float rotationX, rotationY;
    public bool shipActivated {get; private set;} = true;
    Quaternion targetRot;
    
    protected override void Start() 
    {
        base.Start();
        targetRot = transform.rotation;
    }
    
    private void Update()
    {
        if (!shipActivated) return;
        GetInputs();
    }

    public void Activate(bool state)
    {
        shipActivated = state;
    }

    void GetInputs()
    {
        foward = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetKey(KeyCode.Space) ? 1 : Input.GetKey(KeyCode.Space) ? -1 : 0;


        mouseX = Input.GetKey(KeyCode.LeftShift) ?  Input.GetAxisRaw("Mouse X") : 0;
        mouseY = Input.GetKey(KeyCode.LeftShift) ?  -Input.GetAxisRaw("Mouse Y") : 0;
        rollInput = Input.GetKey(KeyCode.E) ? -1 : Input.GetKey(KeyCode.Q) ? 1 : 0;
    }

    public override void AddForce(Vector3 force, Body formBody)
    {
        base.AddForce(force, formBody);
    }

    private void FixedUpdate()
    {
        //if (!shipActivated) return;
        ManageMovement(Time.fixedDeltaTime);
    }

    void ManageMovement(float deltaTime){
        mouseX *= mouseSensitivity;
        mouseY *= mouseSensitivity ;
        rollInput *= mouseSensitivity;
        
        //manage pos movement
        Vector3 movement = (transform.forward * foward + transform.right * horizontal + transform.up * vertical);
        rb.AddForce(movement* acceleration, ForceMode.Acceleration);
        rb.AddTorque(transform.up * mouseX + transform.right * mouseY + transform.forward * rollInput, ForceMode.Acceleration);
    }

    void ManageRot(){


        Quaternion horizontalRot = Quaternion.AngleAxis(mouseX, transform.up);
        Quaternion verticalRot = Quaternion.AngleAxis(-mouseY, transform.right);
        Quaternion rollRot = Quaternion.AngleAxis(-rollInput, transform.forward);
        targetRot *= (rollRot * verticalRot * horizontalRot);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotVelocity * Time.deltaTime);

    }

}
