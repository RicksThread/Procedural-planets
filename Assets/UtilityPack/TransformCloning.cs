using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityPack;

public class TransformCloning : MonoBehaviour
{
    [SerializeField] TransformCloneSettings[] cloneSettings;

    private void Start() {
        TransformCloningManager cloningManager = this.gameObject.GetComponentFromParents<TransformCloningManager>();
        cloningManager.LoadTransformToClone(cloneSettings);
    }

    
}
