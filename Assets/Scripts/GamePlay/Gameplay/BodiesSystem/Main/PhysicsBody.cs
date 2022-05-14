using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///Extension of the body base class that uses the rigidbody physics unity's implementation
///</summary>
[RequireComponent(typeof(Rigidbody))]
public class PhysicsBody : Body
{
    public Rigidbody rb {get; private set;}
    public bool useGravity;

    protected override void Awake()
    {
        base.Awake();
        //get the rigidbody and sets its settings
        rb = GetComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.mass = mass;
        rb.drag = 0;
    }
    
    public override void AddForce(Vector3 force, Body fromBody)
    {   
        base.AddForce(force,fromBody);
        //convert the body addforce in a rigidbody addforce
        rb.AddForce(force);
    }


    protected override void BodyPhysicsFixedUpdate()
    {
    }

}
