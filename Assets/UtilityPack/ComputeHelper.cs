using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityPack
{
    public static class ComputeHelper
    {
        ///<summary>
        ///Dispatches the computeshader with the given number of iterations
        ///</summary>
        public static void Run(ComputeShader computeShader, int kernelIndex, int iterationsX, int iterationsY, int iterationsZ)
        {
            uint xThreads, yThreads, zThreads;

            computeShader.GetKernelThreadGroupSizes(kernelIndex, out xThreads, out yThreads, out zThreads);
            
            xThreads = (uint)(Mathf.CeilToInt(iterationsX / (int)xThreads) + 1);
            yThreads = (uint)(Mathf.CeilToInt(iterationsY / (int)yThreads) + 1);
            zThreads = (uint)(Mathf.CeilToInt(iterationsZ / (int)zThreads) + 1);

            computeShader.Dispatch(kernelIndex, (int)xThreads, (int)yThreads, 1);
        }
    }
}
