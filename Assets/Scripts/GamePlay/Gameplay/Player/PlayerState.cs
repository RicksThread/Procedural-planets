using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///Manages all the player's classes' inputs and states 
///</summary>
public class PlayerState : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PhysicsBody body;
    [SerializeField] private MoveController moveController;
    [SerializeField] private CamController camController;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private RotateTowards rotateTowards;
    [SerializeField] private Transform orientation;

    private void Start()
    {
        Application.targetFrameRate = 144;
        Cursor.lockState =  CursorLockMode.Locked;
    }

    public void EnablePhysics(bool state){
        if (!state){
            body.EnabledBody(false);
            GetComponent<Rigidbody>().isKinematic = true;
            moveController.EnableInput(false);
        }else{
            body.EnabledBody(true);
            GetComponent<Rigidbody>().isKinematic = false;
            moveController.EnableInput(true);
        }
    }

    public void EnableMovement(bool state){
        moveController.EnableInput(state);
    }

    public void EnableRotation(bool state){
        camController.EnableInput(state);
    }

    public void EnableRotationTowardsPlanets(bool state){
        rotateTowards.enabled = state;
    }

    public void SetOrientation(Quaternion orientation){
        this.transform.rotation = orientation;
        this.orientation.localRotation = Quaternion.identity;
    }

    public void SetToDefaultVisionRotCam(){
        camController.SetCamXAxisRot(0);
        
    }
}
