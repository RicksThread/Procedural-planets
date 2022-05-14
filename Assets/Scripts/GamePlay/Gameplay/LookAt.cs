using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform target;
    public Transform from;
    public Transform transfToRotate;
    public float offSetThreshhold = 1f;

    private void FixedUpdate() {
        Vector3 direction = target.position-from.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized,transfToRotate.up);
        float angleMagnitude = Quaternion.Angle(transfToRotate.rotation, lookRotation);
        if (angleMagnitude > offSetThreshhold){
            transfToRotate.rotation = lookRotation;
        }
    }
}
