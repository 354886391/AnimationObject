using UnityEngine;


public class WaitForComplete : CustomYieldInstruction
{
    private bool _loop;
    private float _time;
    private float _duration;

    public WaitForComplete(float duration, bool loop)
    {
        _loop = loop;
        _time = 0.0f;
        _duration = duration;
    }

    public override bool keepWaiting
    {
        get
        {
            if (_loop || _time < _duration)
            {
                _time += Time.deltaTime;
                return true;
            }
            return false;
        }

    }

    public bool Loop { get => _loop; set => _loop = value; }
}
