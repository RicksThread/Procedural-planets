using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformUtilities 
{
    ///<summary>
    ///It gets the angular difference in degrees from the previous rotation to the current rotation 
    ///</summary>
    public static Vector3 GetAngularDifference(Quaternion currentRotation,Quaternion previousRotation)
    {
        //calculate the delta rotation in quaternion
        Quaternion deltaRot = currentRotation * Quaternion.Inverse( previousRotation );
        
        //convert the value in degrees and in to a vector3 variable
        Vector3 eulerRot = new Vector3(
            Mathf.DeltaAngle( 0, deltaRot.eulerAngles.x ), //x
            Mathf.DeltaAngle( 0, deltaRot.eulerAngles.y ), //y
            Mathf.DeltaAngle( 0, deltaRot.eulerAngles.z ) ); //z
        
        return eulerRot; //return value
    }
}
