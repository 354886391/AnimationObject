using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

public class FrameAnimation : SerializedMonoBehaviour
{
    [SerializeField, ReadOnly]
    private int _frameLoop = 1;
    [SerializeField, ReadOnly]
    private float _frameRate = 30;
    private float _frameSpeed;
    private float _frameTimer;

    [SerializeField, ReadOnly]
    private int _currFrame = 0;
    private int _prevFrame = -1;

    [SerializeField, ReadOnly]
    private FrameClip _frameClip;
    [SerializeField, ReadOnly]
    private FrameState _frameState;
    [SerializeField, ReadOnly]
    private SpriteRenderer _renderer;
    [OdinSerialize, ShowInInspector]
    private Dictionary<string, FrameClip> _clipList;
    public System.Action OnCompleted;
    public System.Action<FrameClip, int> EventTriggered;

    public bool Playing
    {
        get { return (_frameState & FrameState.Playing) != 0; }
    }

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public bool IsPlaying(FrameClip clip)
    {
        return Playing && _frameClip != null && _frameClip == clip;
    }

    public void Play(string key, float speed, int loop)
    {
        if (_clipList != null && _clipList.ContainsKey(key))
        {
            Play(_clipList[key], 0, 0, speed, loop);
        }
    }

    public void Play(string key, float fps, float speed, int loop)
    {
        if (_clipList != null && _clipList.ContainsKey(key))
        {
            Play(_clipList[key], _currFrame, fps, speed, loop);
        }
    }

    public void Play(string key, int startFrame, float fps, float speed, int loop)
    {
        if (_clipList != null && _clipList.ContainsKey(key))
        {
            Play(_clipList[key], startFrame, fps, speed, loop);
        }
    }

    public void Play(FrameClip clip, float startTime, float fps, float speed, int loop)
    {
        if (clip != null)
        {
            var frameRate = fps > 0 ? fps : clip.fps;
            if (IsPlaying(clip))
            {
                _frameRate = frameRate;
                _frameSpeed = speed;
                _frameLoop = loop;
            }
            else
            {
                _frameState |= FrameState.Playing;
                _frameRate = frameRate;
                _frameSpeed = speed;
                _frameLoop = loop;
                _frameClip = clip;
                if (startTime >= clip.length)
                {
                    _frameState &= ~FrameState.Playing;
                    WrapFrameToTimer(clip.length - 1);
                }
                else
                {
                    WrapFrameToTimer(startTime);
                }
            }
        }
    }

    public void Stop(bool complete)
    {
        _frameState &= ~FrameState.Playing;
        if (complete) WrapFrameToTimer(_frameClip.length - 1);
    }

    private void SetSprite(Sprite sprite)
    {
        if (_renderer != null && _renderer.sprite != sprite)
        {
            _renderer.sprite = sprite;
            //Debug.Log("sprite: " + sprite.name + " index: " + _currFrame);
        }
    }

    private void SetFrame(int currFrame)
    {
        if (_prevFrame != currFrame)
        {
            SetSprite(_frameClip.frames[currFrame].sprite);
            _prevFrame = currFrame;
        }
    }

    private void WrapFrameToTimer(float time)
    {
        _frameTimer = time;
        _currFrame = (int)_frameTimer % _frameClip.length;
        SetSprite(_frameClip.frames[_currFrame].sprite);
        ProcessEvents(_currFrame - 1, _currFrame);
        _prevFrame = _currFrame;
    }

    private void ProcessEvents(int start, int last)
    {
        if (EventTriggered == null)
        {
            return;
        }
        for (int frame = start + 1; frame != (last + 1); frame++)
        {
            if (_frameClip.frames[frame].triggerEvent && EventTriggered != null)
            {
                EventTriggered(_frameClip, frame);
            }
        }
    }

    private void UpdateAnimation(float deltaTime)
    {
        if (_frameState != FrameState.Playing)
        {
            return;
        }
        _frameTimer += _frameRate * _frameSpeed * deltaTime;    // ¶³Ö¡Ê±, Í£Ö¹ÀÛ¼Ó       
        int prevFrame = _prevFrame;
        _currFrame = (int)_frameTimer;

        if (_currFrame >= _frameClip.length)// ¶¯»­Ä©Î²
        {
            if (_frameLoop == 1)
            {
                _frameState &= ~FrameState.Playing;
                _currFrame = _frameClip.length - 1;
                SetFrame(_currFrame);
                ProcessEvents(prevFrame, _currFrame);
            }
            else
            {
                _frameLoop--;
                _prevFrame = -1;
                _frameTimer = _currFrame = 0;
                SetFrame(_currFrame);
                ProcessEvents(-1, _currFrame);
            }
        }
        else
        {
            SetFrame(_currFrame);
            ProcessEvents(prevFrame, _currFrame);
        }
    }

    public virtual void LateUpdate()
    {
        UpdateAnimation(Time.deltaTime);
    }

    public enum FrameState
    {
        Init = 0,
        Playing = 1,
        Paused = 2,
    }

}
