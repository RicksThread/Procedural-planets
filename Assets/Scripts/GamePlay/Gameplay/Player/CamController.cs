using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    //settings of the cam: sensitivity and maxminRot for the camera in the x axis 
    [System.Serializable]
    public class Settings
    {
        public float xSensitivity;
        public float ySensitivity;

        public float xRotateMax;
        public float xRotateMin;
    }
    //instance to store all the settings of the camera controller
    public Settings settings;
    
    public Transform cam; //transform of the real cam where the rotation of the x axis will be applied
    public Transform camParent; //parent of the real camera where the rotation of the orientation will be applied
    public Transform orientation; //camera orientation of the y axis

    //store the xRotation of the cam
    float xRotationCam;
    //store the inputs from the updateMethod
    float inputX = 0;
    float inputY = 0;

    public bool takeInput {get; private set; } = true;

    private void Start() 
    {
        xRotationCam = cam.localEulerAngles.x;
    }

    private void LateUpdate()
    {
        Rotate(Time.deltaTime);
    }

    ///<summary>
    /// It takes the input to then rotate the camera
    ///</summary>
    public void InputRotate(Vector2 rotInput)
    {
        if (!takeInput) return;
        //get the inputs
        inputX = rotInput.x;
        inputY = rotInput.y;
    }

    void Rotate(float deltaTime)
    {
        //calculate the delta of the y axis rotation
        float yRotation = inputX * settings.xSensitivity * deltaTime;

        xRotationCam  -=inputY * settings.ySensitivity * deltaTime;
        xRotationCam = Mathf.Clamp(xRotationCam, settings.xRotateMin, settings.xRotateMax);

        //rotate the body
        orientation.Rotate(Vector3.up,yRotation);

        //set the x local rotation of the camera to the xRotationCam
        camParent.rotation = orientation.rotation;
        cam.localRotation = Quaternion.Euler(xRotationCam,cam.localEulerAngles.y,cam.localEulerAngles.z);
    }

    public void EnableInput(bool state)
    {
        if (!state){
            inputX = 0;
            inputY = 0;
        }
        takeInput = state;
    }

    public void SetDefaultView()
    {
        camParent.rotation = orientation.rotation;
        cam.localRotation = Quaternion.identity;
    }

    public void SetCamXAxisRot(float angle)
    {
        xRotationCam = angle;
        cam.localRotation = Quaternion.Euler(angle,cam.localEulerAngles.y,cam.localEulerAngles.z);
    }

}
