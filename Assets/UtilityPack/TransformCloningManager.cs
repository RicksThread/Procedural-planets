using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformCloningManager : MonoBehaviour
{
    [SerializeField] TransformClone[] transformsClone;
    List<TransformClone> transformsToClone = new List<TransformClone>();
    public int lengthClones {get { return transformsClone.Length; }}

    private void Update() {
        for (int i = 0; i < transformsClone.Length; i++)
        {
            transformsClone[i].UpdateClone();
        }
    }

    public void LoadTransformToClone(TransformCloneSettings[] transformsToClone){
        this.transformsToClone.Clear();
        bool success;
        for (int i = 0; i < transformsToClone.Length; i++)
        {
            for (int k = 0; k < transformsClone.Length; k++)
            {
                transformsClone[k].SetTransformToClone(transformsToClone[k], out success);

                if (success) break;
            }
        }
    }
}

[System.Serializable]
public class TransformClone{
    public TransformCloneSettings cloneSettings;
    Transform transformToClone;

    public void SetTransformToClone(TransformCloneSettings clone, out bool success){
        success = false;
        if (clone.nameReference != cloneSettings.nameReference) return;

        transformToClone = clone.transform;
    }

    public void UpdateClone(){
        if (transformToClone == null) return;
        if (cloneSettings.transform == null) return;

        cloneSettings.transform.position = transformToClone.position;
        cloneSettings.transform.rotation = transformToClone.rotation;
    }
}

[System.Serializable]
public struct TransformCloneSettings{
    public string nameReference;
    public Transform transform;
}
