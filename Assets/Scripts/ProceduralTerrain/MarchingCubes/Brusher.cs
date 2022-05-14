using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UtilityPack.InputSystem;
using UtilityPack;
public class Brusher : MonoBehaviour
{
    [Header("General settings")]
    [SerializeField] private PlanetChunkWorld world;
    [SerializeField] private LayerMask layerGround;
    
    [Header("Brush settings")]
    [SerializeField] private float radiusBrush;
    [SerializeField] private float amountBrush;

    [Header("Dig settings")]
    [SerializeField] private float amountDig;
    [SerializeField] private float radiusDig;
    
    [SerializeField] private bool dummy;

    [Header("Constraints")]
    [SerializeField] private float minDstBrush = 1f;
    [SerializeField] private float maxDstBrush = 50f;

    //sphere collider used to negate passing through the collider of the world
    
    [Header("Collider fixing settings")]
    [SerializeField] private SphereCollider brusherColliderController;
    [SerializeField] private float ColliderControllerRadius;
    [SerializeField] private float timerToDisableCollider;

    private PlayerInputs playerInputs;
    private InputControllerButton brusherInputController;
    private InputControllerButton changeStateBrusherController;
    private TimeStateHandler timeStateCollider;

    private bool isBrushing = false;
    private bool isDigging = false;

    private void Awake() 
    {
        playerInputs = new PlayerInputs();
        playerInputs.Enable();    
        playerInputs.BaseMovement.Enable();

        brusherInputController = new InputControllerButton(playerInputs.BaseMovement.Use);
        changeStateBrusherController = new InputControllerButton(playerInputs.BaseMovement.ChangeState);

        timeStateCollider = new TimeStateHandler(false);
    }

    private void Start()
    {
        brusherColliderController.radius = ColliderControllerRadius;    
    }

    private void Update()
    {
        isBrushing = brusherInputController.GetInput();
        if (changeStateBrusherController.GetInputStart()) 
            isDigging = !isDigging;
    }

    private void FixedUpdate()
    {
        if (timeStateCollider.state)
        {
            brusherColliderController.enabled = true;
        }
        else
        {
            brusherColliderController.enabled = false;
        }

        if (isBrushing)
        {
            if (isDigging)
                Brush(radiusDig, -amountDig * Time.fixedDeltaTime);   
            else
                Brush(radiusBrush, amountBrush * Time.fixedDeltaTime);   
        }
    }

    private void Brush(float radius, float amount)
    {
        RaycastHit rayInfo;
        if (Physics.Raycast(transform.position, transform.forward, out rayInfo, Mathf.Infinity, layerGround, QueryTriggerInteraction.Ignore))
        {   
            Debug.Log("Brush ray hit name: " + rayInfo.transform.name);
            Vector3 DirToPoint = rayInfo.point - transform.position;
            float dstPoint = DirToPoint.magnitude;
            
            if (dstPoint < minDstBrush || dstPoint > maxDstBrush) return;

            world.Brush(rayInfo.point, radius, amount * Time.fixedDeltaTime);
            brusherColliderController.transform.position = transform.position + DirToPoint.normalized * (dstPoint + ColliderControllerRadius+0.05f);
            brusherColliderController.enabled = true;
            timeStateCollider.ChangeState(timerToDisableCollider, true);
        }
    }
}
