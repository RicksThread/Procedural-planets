using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
    void Damage(float value);
    void Regen(float value);
}
