using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

namespace UtilityPack
{
    public class TimeStateHandler 
    {
        public bool state {get; private set;}
        private bool defaultState;

        float time = 0;
        float timeDelay = 0;
        public TimeStateHandler(bool defaultState)
        {
            this.defaultState = defaultState;
            state = defaultState;
        }

        public void ChangeState(float timeFrame, bool reset = false)
        {
            if (reset){
                time = 0;
                timeDelay = timeFrame;
            }
            if (state == defaultState) Timing.RunCoroutine(DelaySetState());
            state = !defaultState;
        }

        IEnumerator<float> DelaySetState()
        {
            while(time < timeDelay)
            {
                time+= Time.deltaTime;
                yield return Timing.WaitForSeconds(Time.deltaTime);
            }
            state = defaultState;
        }

    }
}
