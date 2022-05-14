using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///It coordinates the actions of each body in the scene
///</summary>
public class BodiesManager : MonoBehaviour
{
    public virtual void Awake() {
        //find each duplicate of the the bodiesManager and destroy them
        BodiesManager[] managers = FindObjectsOfType<BodiesManager>();
        foreach(BodiesManager manager in managers){
            if (manager != this) {
                Destroy(manager.gameObject);
                Debug.Log("MULTIPLE INSTACES DETECTED AND REMOVED");
            }
        }
    }

    //Call each body Fixedupdate method
    public virtual void FixedUpdate() {
        foreach(Body body in Body.bodies){
            body.BodyFixedUpdate();
        }
    }
}