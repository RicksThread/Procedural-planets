using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//it drags the bodies along the position and rotation of the celestial body
//NOTE: imagine that the planet is rotating, then it's assumed that all the objects on the surface follows that rotation
public class CelestialBodyDrag : MonoBehaviour
{
    public float heightStartDrag;
    public float heightMaxDrag;

    [SerializeField] Celestial.CelestialBody celestialBody;

    private void FixedUpdate()
    {
        // iterate through all the bodies (except for the celestial body)
       for(int i = 0; i < Body.bodies.Count; i++){
           if(Body.bodies[i] != celestialBody && !Body.bodies[i].isChildToBody)ManageBody(Body.bodies[i]);
       }
    }

    private void ManageBody(Body body)
    {
        Vector3 bodyLocalPosition = (body.transform.position - transform.position);
        float height = bodyLocalPosition.magnitude;

        //if the distance from the celestial body is less than the start attrition the drag them with the atmosphere
        if (height < heightStartDrag && body.transform.parent != this.transform){

            //calculate the height clamped from 0 to 1 and inverse it, as the drag is inversely proportional to the height
            float magnitudeDrag = 1-(Mathf.Max(height-heightMaxDrag,0))/(heightStartDrag-heightMaxDrag); 
            Vector3 offSetBody = GetRotOffSetPos(bodyLocalPosition) * Time.fixedDeltaTime + celestialBody.velocities.velocity * Time.fixedDeltaTime;
            
            
            body.MoveTransform(offSetBody * magnitudeDrag, celestialBody, false);
            body.transform.rotation *= Quaternion.Euler(celestialBody.velocities.angularVelocity* Time.fixedDeltaTime);
        }
    }

    Vector3 GetRotOffSetPos(Vector3 localPos)
    {
        return Quaternion.Euler(celestialBody.velocities.angularVelocity) * localPos - localPos; 
    }
}
