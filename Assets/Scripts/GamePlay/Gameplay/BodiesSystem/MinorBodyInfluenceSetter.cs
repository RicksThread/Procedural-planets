using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//set the minor body influece based on the collision detection properties
public class MinorBodyInfluenceSetter : MonoBehaviour
{
    [SerializeField] protected MinorBody minorBody;
    [Header("Detection properties")]
    [SerializeField] protected LayerMask layerBodyToStick;

    //manage collisions and triggers
    protected virtual void OnTriggerEnter(Collider other)
    {
        OnEnterColliderCheckBody(other);
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        OnEnterColliderCheckBody(other.collider);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        OnExitColliderCheckBody(other);
    }

    protected virtual void OnCollisionExit(Collision other)
    {
        OnExitColliderCheckBody(other.collider);
    }

    protected virtual void OnEnterColliderCheckBody(Collider collider)
    {
        
        //check if the actual gameobject's collider has the physics body and respects the layer requirement
        if (IsInLayerMask(collider.gameObject, layerBodyToStick))
        {
            if (collider.GetComponent<Rigidbody>() != null)
            {
                minorBody.SetBodyInfluenced(collider.GetComponent<Rigidbody>());
            }
        }

        //check if the the collider's parents have a physics body and if the collider respects the layer required 
        if (IsInLayerMask(collider.gameObject, layerBodyToStick) && !minorBody.isBodyInfluenced)
        {
            Transform parentOther = collider.transform.parent;
            int i = 0;
            while(parentOther != null && i < 20)
            {
                if (parentOther.gameObject.GetComponent<Rigidbody>() != null)
                {
                    minorBody.SetBodyInfluenced(parentOther.gameObject.GetComponent<Rigidbody>());
                    break;
                }
                i++;
                parentOther = parentOther.parent;
            }
        }

    }

    protected virtual void OnExitColliderCheckBody(Collider collider)
    {
        
        //check if the actual gameobject's collider has the physics body and respects the layer requirement
        if (collider.GetComponent<PhysicsBody>() != null)
        {
            if (minorBody.CompareBodyInfluenced(collider.GetComponent<PhysicsBody>()))
            {
                minorBody.SetBodyInfluenced(null);
            }
        }

        //check if the the collider's parents have a physics body and if the collider respects the layer required 
        if (IsInLayerMask(collider.gameObject, layerBodyToStick) && minorBody.isBodyInfluenced)
        {
            Transform parentOther = collider.transform.parent;
            int i = 0;
            while(parentOther != null && i < 20)
            {
                if (minorBody.CompareBodyInfluenced(parentOther.gameObject.GetComponent<PhysicsBody>())){
                    minorBody.SetBodyInfluenced(null);
                    break;
                }
                i++;
                parentOther = parentOther.parent;
            }
        }
        
    }

    protected bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << obj.layer)) > 0);
    }
}
