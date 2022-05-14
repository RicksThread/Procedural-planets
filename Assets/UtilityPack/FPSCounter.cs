using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MEC;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI FPSText;
    [SerializeField] private float timeUpdate;
    int nFrames = 0;
    bool calculate = true;
    private void Update() {
        nFrames++;
        if (calculate)
            Timing.RunCoroutine(TimeLapse().CancelWith(this.gameObject));
    }

    IEnumerator<float> TimeLapse()
    {
        calculate = false;
        yield return Timing.WaitForSeconds(timeUpdate);
        FPSText.text = ((float)nFrames/timeUpdate).ToString();
        nFrames = 0;
        calculate = true;
    }
}
