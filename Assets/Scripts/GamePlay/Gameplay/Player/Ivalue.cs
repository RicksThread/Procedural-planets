using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Ivalue : MonoBehaviour
{
    public abstract event Action<float> OnValueChange;
    public abstract float GetValue();

}
