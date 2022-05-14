using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowards : MonoBehaviour
{
    [Header("Rotation towards")]
    public LayerMask surface;
    public Transform[] objects;
    //base rotating speed of the player
    public float rotSpeedToTransform;
    //distance from surface to start the rotation of the player
    public float dstSurfStartRot;

    private void FixedUpdate() {    
        if (objects.Length > 0){
            Vector3 planetPos = GetNearestPlanetPos();
            RotateTowardsPlanet(planetPos);
        }
    }

    //it gets the nearest planet's position with a simple algorithm
    Vector3 GetNearestPlanetPos(){
        int index = 0;
        for (int i = 0; i < objects.Length; i++)
        {
            float lastDistance = (transform.position- objects[i].transform.position).magnitude;
            if ((transform.position- objects[i].transform.position).magnitude < lastDistance){
                index = i;
            }
        }
        if(objects.Length > 0)return objects[index].transform.position;
        return Vector3.zero;
    }

    //It rotates the current transform so that the local up orientation faces the planet's center
    void RotateTowardsPlanet(Vector3 planetPos){
        //get direction and distance from planet to player
        Vector3 direction = planetPos - transform.position;
        float distance = direction.magnitude;


        //create raycast to store information about the surface
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction.normalized, out hit,distance,surface)){
            float radius = (planetPos-hit.point).magnitude;
            if (distance <  radius + dstSurfStartRot){
                float distanceFromSurface = Mathf.Max(distance-radius,0);

                //the more it's distant to the surface the less the rotation
                //it increases as the distance decreases
                float velocity = (1-(distanceFromSurface/dstSurfStartRot)) * rotSpeedToTransform;

                //get the desired rotation from player to planet
                Quaternion lookAt = Quaternion.FromToRotation(-transform.up, direction.normalized);
                //Set a smooth transition form current rotation to lookAt
                transform.rotation = Quaternion.Slerp(transform.rotation, lookAt * transform.rotation, velocity* Time.fixedDeltaTime);
            }
        }
    }
}
