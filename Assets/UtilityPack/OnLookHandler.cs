using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityPack;

public class OnLookHandler<T>
{
    //main settings of the lookColliderSystem
    private Transform origin;
    private LayerMask mask;
    private float dst;
    private string tag;

    public delegate void OnLooking(RaycastHit hit, T component);
    private delegate bool CheckColliderDelegate(out RaycastHit hit, out T component);

    //delegates to store the block of code for each situation
    OnLooking WhileLooking;
    OnLooking OnStartLooking;
    OnLooking OnEndLooking;

    CheckColliderDelegate checkColliderSystem;

    bool wasLooking = false;

    ///<summary>
    ///
    ///</summary>
    public OnLookHandler(Transform origin, LayerMask mask, float dst, OnLooking OnStartLooking, OnLooking WhileLooking, OnLooking OnEndLooking, string tag ="")
    {
        this.origin = origin;
        this.mask = mask;
        this.dst = dst;
        this.tag = tag;
        this.WhileLooking = WhileLooking;
        this.OnStartLooking = OnStartLooking;
        this.OnEndLooking = OnEndLooking;
        checkColliderSystem = CheckColliderMask;
    }

    ///<summary>
    ///
    ///</summary>
    public OnLookHandler(Transform origin, float dst, OnLooking OnStartLooking, OnLooking WhileLooking, OnLooking OnEndLooking, string tag ="")
    {
        this.origin = origin;
        this.dst = dst;
        this.tag = tag;
        this.WhileLooking = WhileLooking;
        this.OnStartLooking = OnStartLooking;
        this.OnEndLooking = OnEndLooking;
        checkColliderSystem = CheckCollider;
    }

    public void Tick()
    {
        RaycastHit hitInfo;
        T component;
        bool isLooking = checkColliderSystem(out hitInfo, out component);
        if (isLooking)
        {
            if (isLooking != wasLooking){
                if (isLooking)
                    if(OnStartLooking != null)
                        OnStartLooking(hitInfo,component);
                else
                    if(OnEndLooking != null)
                        OnEndLooking(hitInfo, component);
                wasLooking = isLooking;
            }
            if(WhileLooking != null)
                WhileLooking(hitInfo,component);
        }
    }

    bool CheckColliderMask(out RaycastHit hit, out T component)
    {
        component = default;
        if (Physics.Raycast(origin.position,origin.forward, out hit, dst, mask))
        {
            if (tag == "")
            {
                component = hit.collider.gameObject.GetComponentFromParents<T>();
                if (component != null)
                {
                    return true;
                }
            }
            else
            {
                if (hit.collider.CompareTag(tag))
                {
                    component = hit.collider.gameObject.GetComponentFromParents<T>();
                    if (component != null)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    bool CheckCollider(out RaycastHit hit, out T component)
    {
        component = default;
        if (Physics.Raycast(origin.position,origin.forward, out hit, dst))
        {
           if (tag == "")
            {
                component = hit.collider.gameObject.GetComponentFromParents<T>();
                if (component != null)
                {
                    return true;
                }
            }
            else
            {
                if (hit.collider.CompareTag(tag))
                {
                    component = hit.collider.gameObject.GetComponentFromParents<T>();
                    if (component != null)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}