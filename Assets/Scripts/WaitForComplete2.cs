using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForComplete2 : CustomYieldInstruction
{
    private float _time;
    private float _duration;

    public WaitForComplete2(float duration)
    {
        this._time = 0.0f;
        this._duration = duration;
    }

    public override bool keepWaiting
    {
        get
        {
            if (_time < _duration)
            {
                _time += Time.deltaTime;
                return true;
            }
            return false;
        }
    }


}
