using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MEC;

///<summary>
///It makes sure to call the action in the mainThread even from a backGround non safe threaded operation
///</summary>
public class SafeThreadEventHandler<T> : IDisposable where T : EventArgs 
{
    public event EventHandler<T> Holder;
    private bool isReady = false;
    private T args;
    private object sender;

    public SafeThreadEventHandler(object sender, EventHandler<T> actionHandler)
    {
        this.sender = sender;
        Holder+=(obj, args)=> actionHandler?.Invoke(obj, args);
        Timing.RunCoroutine(DelayerController());
    }

    //clock of the main thread
    //calls the event when it's called safely
    IEnumerator<float> DelayerController()
    {
        while(!isReady)
        {
            yield return Timing.WaitForSeconds(0);
        }

        Holder?.Invoke(sender, args);
        Dispose();
    }

    ///<summary>
    ///Executes the event safely on the main thread
    ///</summary>
    public void Execute(T args)
    {
        this.args = args;
        //warns the object that it's ready to call the event
        isReady = true;
    }

    public void Dispose()
    {
        Holder = null;
        args = null;
        sender = null;
    }
}


///<summary>
///It makes sure to call the action in the mainThread even from a backGround non safe threaded operation
///</summary>
public class SafeThreadAction : IDisposable
{
    public event Action Holder;
    private bool isReady = false;
    private bool disposeOnDone = false;

    public SafeThreadAction(Action actionHandler)
    {
        Holder+=()=> actionHandler?.Invoke();
        Timing.RunCoroutine(DelayerController());
    }

    //clock of the main thread
    //calls the event when it's called safely
    IEnumerator<float> DelayerController()
    {
        while(!isReady)
        {
            yield return Timing.WaitForSeconds(0);
        }
        Holder?.Invoke();
        if (disposeOnDone) Dispose();
    }

    ///<summary>
    ///Executes the event safely on the main thread
    ///</summary>
    public void Execute(bool disposeOnDone)
    {
        //warns the object that it's ready to call the event
        isReady = true;
        this.disposeOnDone = disposeOnDone;
    }

    public void Dispose()
    {
        Holder = null;
    }
}