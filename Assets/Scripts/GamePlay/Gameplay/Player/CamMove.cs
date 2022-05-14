using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//It moves the camera to the target
[ExecuteAlways]
public class CamMove : MonoBehaviour
{
    public Transform target;

    private void LateUpdate()
    {
        transform.position = target.position;
    }
}
