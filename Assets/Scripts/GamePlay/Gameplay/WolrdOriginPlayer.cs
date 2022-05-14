using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WolrdOriginPlayer : MonoBehaviour
{
    public float dstThresholdSetOrigin;
    public float timeToAdjustOrigin;

    private void FixedUpdate() {
        UpdateOriginPlayer();
    }

    void UpdateOriginPlayer(){
        Vector3 directionToOrigin = -transform.position;
        if(transform.position.magnitude > dstThresholdSetOrigin){
            Debug.Log("sus");
            foreach(Body rootGameobject in Body.bodies){
                if(!rootGameobject.isChildToBody)rootGameobject.MoveTransform(directionToOrigin, null, true);
            }
        }
    }
}
