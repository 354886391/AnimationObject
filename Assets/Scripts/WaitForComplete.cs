using UnityEngine;


public class WaitForComplete : CustomYieldInstruction
{
    private float _time;
    private float _duration;

    public WaitForComplete(float duration)
    {
        _time = 0.0f;
        _duration = duration;
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
