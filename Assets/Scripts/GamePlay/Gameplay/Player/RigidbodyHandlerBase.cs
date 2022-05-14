using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyHandlerBase : MonoBehaviour, IRigidbodyHandler
{
    [SerializeField] private Rigidbody rb;

    public void AddForce(Vector3 force, ForceMode forceMode)
    {
        rb.AddForce(force, forceMode);
    }

    public float GetAngularDrag()
    {
        return rb.angularDrag;
    }

    public float GetDrag()
    {
        return rb.drag;
    }

    public Vector3 GetRotationVelocity()
    {
        return rb.angularVelocity;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    public void SetDrag(float drag)
    {
        rb.drag = drag;
    }

    public void SetRotationalDrag(float drag)
    {
        rb.angularDrag = drag;
    }

    public void SetVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;
    }
}
