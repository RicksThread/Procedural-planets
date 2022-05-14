using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class AsyncGPUMultiRequest
{
    public delegate void AsyncMultiGPUOperation(AsyncGPUReadbackRequest[] requests);
    private ComputeBuffer[] buffers;

    private Dictionary<int, int> buffersSizeToIndex = new Dictionary<int, int>();
    private List<AsyncGPUReadbackRequest> requestsListAsync = new List<AsyncGPUReadbackRequest>();
    private AsyncMultiGPUOperation operation;

    public AsyncGPUMultiRequest(ComputeBuffer[] buffers, AsyncMultiGPUOperation requestMethod)
    {
        this.buffers = buffers;
        this.operation = requestMethod;

        for (int i = 0; i < buffers.Length; i++)
        {
            buffersSizeToIndex.Add(buffers[i].stride * buffers[i].count,i);
        }
    }

    public void Request()
    {
        for(int i = 0; i < buffers.Length; i++)
        {
            AsyncGPUReadback.Request(buffers[i], LoadBuffer);
        }
    }

    private void LoadBuffer(AsyncGPUReadbackRequest request)
    {
        requestsListAsync.Add(request);
        if (requestsListAsync.Count == buffers.Length)
        {
            MethodCallBack();
        }
    }

    private void MethodCallBack()
    {
        AsyncGPUReadbackRequest[] requests = new AsyncGPUReadbackRequest[buffers.Length];

        for(int i = 0; i < buffers.Length; i++)
        {
            int requestIndex = buffersSizeToIndex[requestsListAsync[i].layerDataSize];
            requests[requestIndex] = requestsListAsync[i];
        }
        //call operation
        operation(requests);
    }

    public void Dispose()
    {
        operation = null;
    }
}
