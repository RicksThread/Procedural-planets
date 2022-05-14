using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
{
    //static variable where it holds the instance of the object
    public static T instance;


    protected virtual void Awake()
    {
        //check if there's an instance of the class type T
        if (this.GetComponent<T>() == null)
        {
            Debug.LogError("T instance component has not been founded");
            return;
        }
        //if the instance is null or different replace it with the one in this gameobject
        if (instance == null || !ReferenceEquals(instance,this.GetComponent<T>()))
            instance = this.GetComponent<T>();
            
        Destroy(this);
    }
}
