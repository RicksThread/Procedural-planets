using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRigidbodyHandler 
{
    void AddForce(Vector3 force, ForceMode forceMode);
    void SetDrag(float drag);
    void SetRotationalDrag(float drag);
    void SetVelocity(Vector3 velocity);

    Vector3 GetVelocity();
    Vector3 GetRotationVelocity();
    float GetDrag();
    float GetAngularDrag();
}
