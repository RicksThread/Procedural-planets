using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] Transform rayOrigin;
    [SerializeField] float distanceInteract;
    [SerializeField] PlayerState player;

    private void FixedUpdate() 
    {
        RaycastHit hit;
        if(Physics.Raycast(rayOrigin.transform.position, rayOrigin.forward,out hit ,distanceInteract))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (hit.collider.GetComponent<Interactable>() != null)
                {
                    hit.collider.GetComponent<Interactable>() .Interact();
                }
                if (hit.collider.GetComponent<InteractableFrom<PlayerState>>() != null)
                {
                    hit.collider.GetComponent<InteractableFrom<PlayerState>>().InteractFrom(player);
                }
            }
        }
    }
}
