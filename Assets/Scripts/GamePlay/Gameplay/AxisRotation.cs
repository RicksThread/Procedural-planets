using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisRotation : MonoBehaviour
{
    public Vector3 axis;
    public float rotPerSec;
    
    private void FixedUpdate() {
        transform.RotateAround(transform.position, axis,360f * rotPerSec * Time.fixedDeltaTime); 
    }

}
