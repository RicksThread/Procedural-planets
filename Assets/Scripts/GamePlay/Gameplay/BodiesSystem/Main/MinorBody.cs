using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

///<summary>
///Extension of the physics body, It's adds some functionality like localLinearDrag (local to the bodyInfluece)
///</summary>
public class MinorBody : MonoBehaviour 
{
    [SerializeField] protected Rigidbody bodyInfluence;
    [SerializeField] protected Rigidbody bodyInfluenced; 

    [Header("DragToStickedObject")]
    public float localLinearDrag;

    public bool isBodyInfluenced 
    {
        get 
        {
            return bodyInfluence != null; 
        }
    }

    private void Awake()
    {
        
    }

    private void FixedUpdate()
    {
        ApplyDrag(Time.fixedDeltaTime);
    }

    void ApplyDrag(float deltaTime)
    {
        Vector3 currentVelocity = (isBodyInfluenced) ? bodyInfluenced.velocity - bodyInfluence.velocity : bodyInfluenced.velocity;
        bodyInfluenced.AddForce(-currentVelocity.normalized * (currentVelocity.magnitude) * localLinearDrag * deltaTime);
    }

    public void SetBodyInfluenced(Rigidbody bodyInfluence)
    {
        this.bodyInfluence = bodyInfluence;
    }

    public bool CompareBodyInfluenced(PhysicsBody physicsBody)
    {
        return this.bodyInfluence == physicsBody;
    }
}