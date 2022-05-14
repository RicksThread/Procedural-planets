using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using System;

///<summary>
///This class implements the base of a physical movement (force and velocity) and the interaction between each other. Their action will be syncronized
///</summary>
public class Body : MonoBehaviour
{

    public static List<Body> bodies = new List<Body>();
    [SerializeField] private float _mass;
    [SerializeField] protected bool isStatic = false;
    public float mass 
    { 
        get 
        { 
            return _mass; 
        } 
        protected set 
        { 
            _mass = value;
        }
    }

    public bool isChildToBody{
        get{
            return gameObject.GetComponentInParent<Body>() != null;
        }
    }

    ///<summary>
    ///It stores the velocity of the physics movement of the body
    ///<para>NOTE: Its velocity is the sum of all the move vectors passed to the moveMethod with the typemovement of physical</para>
    ///</summary>
    public Vector3 BodyPhysicsVelocity {get; protected set;} = Vector3.zero;

    ///<summary>
    ///Transform's velocities
    ///</summary>
    public Velocities velocities {get; private set;}

    ///<summary>
    ///It enables or disables the bodies function
    ///</summary>
    public bool bodyEnabled {get; private set;} = true;

    protected virtual void Awake(){
        if(FindObjectOfType<BodiesManager>() == null)
        {
            Debug.LogError("BodiesManager is needed to run the bodies!");
        }
    }

    protected virtual void OnEnable() 
    {
        bodies.Add(this);
    }

    protected virtual void Start()
    {
        velocities = new Velocities(this.transform);
    }

    ///<summary>
    ///Called when applying a force to the body object (it changes its velocity)
    ///</summary>
    public virtual void AddForce(Vector3 force,Body fromBody)
    {
        if (!bodyEnabled || isStatic) return;

        BodyPhysicsVelocity += force / mass;
    }

    ///<summary>
    ///It moves the object through space by modifying the transform position
    ///</summary>
    ///<param name="offSetPos"> The delta position that will be applied to the this body</param>
    ///<param name="ignoreVelocity"> It will determine if the current movement will be calculated by the velocity property</param>
    public void MoveTransform(Vector3 offSetPos,Body fromBody, bool IgnoreVelocity = false)
    {
        transform.position += offSetPos;
        if (IgnoreVelocity)
        {
            velocities.OffSetVelocityFrame(offSetPos);
        }
    }

    ///<summary>
    ///Called every fixedUpdate
    ///<summary>
    protected virtual void BodyPhysicsFixedUpdate()
    {
        transform.position +=BodyPhysicsVelocity * Time.fixedDeltaTime;
    }

    ///<summary>
    ///Called everyFrame by the bodiesManager
    ///</summary>
    public virtual void BodyFixedUpdate()
    {
        if (!bodyEnabled) return;
        BodyPhysicsFixedUpdate();
        //calculate the velocity of the current object 
        velocities.UpdateVelocities(Time.fixedDeltaTime);
    }

    public void EnabledBody(bool state){
        bodyEnabled = state;
    }

    protected virtual void OnDisable() {
        bodies.Remove(this);
    }
}

///<summary>
///A class that contains readonly data about the dynamic properties of the transform
///</summary>
public class Velocities
{
    //store the values in these parameters
    ///<summary>
    ///Transform's angularVelocity obtained by the difference of rotation
    ///</summary>
    public Vector3 angularVelocity {get; private set;}
    ///<summary>
    ///Transform's velocity obtained by the difference of position
    ///</summary>
    public Vector3 velocity  {get; private set;}


    public Vector3 localVelocity {get; private set;}
    public Vector3 localAngularVelocity {get; private set; }

    //get the reference of the target object
    private Transform transform;
    bool isLocal = false;

    //store previous values to calculate the delta positions and rotations
    Vector3 previousPos = Vector3.zero;
    Quaternion previousRotation = Quaternion.identity;

    Vector3 velocityOffSetFrame = Vector3.zero;
    Quaternion angularVelocityOffSetFrame = Quaternion.identity;

    Vector3 previosLocalPos = Vector3.zero;
    Quaternion previosLocalRotation = Quaternion.identity;

    public Velocities(Transform transform)
    {
        this.transform = transform;
        previousRotation = transform.rotation;
        previousPos = transform.position;
        previosLocalPos = transform.localPosition;
        previosLocalRotation = transform.localRotation;
    }

    public void UpdateVelocities(float deltaTime)
    {
        //get the angularVelocity
        angularVelocity =  TransformUtilities.GetAngularDifference(transform.rotation, previousRotation)/deltaTime;
        angularVelocity -= angularVelocityOffSetFrame.eulerAngles/deltaTime;
        previousRotation = transform.rotation;

        //get the localAngularVelocity
        localAngularVelocity = TransformUtilities.GetAngularDifference(transform.localRotation, previousRotation)/deltaTime;
        localAngularVelocity -= angularVelocityOffSetFrame.eulerAngles/deltaTime;
        previosLocalRotation = transform.localRotation;
        
        //get velocity
        velocity = (transform.position - previousPos)/deltaTime - velocityOffSetFrame/deltaTime;
        previousPos = transform.position;

        //get local velocity
        if (transform.parent != null)
        {
            if (!isLocal)
            {
                previosLocalPos = transform.localPosition;
                isLocal = true;
            }
            localVelocity = (transform.localPosition - previosLocalPos)/deltaTime + velocityOffSetFrame/deltaTime;
            previosLocalPos = transform.localPosition;
        }else{
            if (isLocal)
            {
                isLocal = false;
            }
            localVelocity = velocity;
        }
        if (velocityOffSetFrame != Vector3.zero) 
            velocityOffSetFrame = Vector3.zero;
        if (angularVelocityOffSetFrame != Quaternion.identity) 
            angularVelocityOffSetFrame = Quaternion.identity;
    
    }

    ///<summary>
    ///Set a different readonly velocity for a physical frame
    ///</summary>
    public void OffSetVelocityFrame(Vector3 offSet)
    {
        velocityOffSetFrame  =offSet;
    }


    ///<summary>
    ///
    ///</summary>
    public void OffSetAngularVelocityFrame(Quaternion offSet)
    {
        angularVelocityOffSetFrame = offSet;
    }
}