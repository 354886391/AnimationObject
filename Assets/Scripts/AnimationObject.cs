using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionNode
{
    public int animIndex;
    public Action action;
    public ActionNode next;

    public ActionNode(int animIndex)
    {
        this.animIndex = animIndex;
        this.action = null;
        this.next = null;
    }
}

public class AnimationObject : MonoBehaviour
{
    [SerializeField]
    private Animation _animation;
    [SerializeField]
    private string[] _animNames;

    private ActionNode _headNode;
    private ActionNode _tailNode;

    private bool _run;
    private bool _loop;
    private float _time;
    private float _duration;

    public ActionNode Current { get { return _headNode; } }


    public AnimationObject Play(int animIndex, bool loop = false)
    {
        return Play(animIndex, 0.0f, loop);
    }

    public AnimationObject Play(int animIndex, float fade, bool loop = false)
    {
        CreateNode(animIndex);
        Animate(animIndex, fade, loop);
        return this;
    }

    public AnimationObject NextPlay(int animIndex, bool loop = false)
    {
        return NextPlay(animIndex, 0.0f, loop);
    }

    public AnimationObject NextPlay(int animIndex, float fade, bool loop = false)
    {
        AddNode(new ActionNode(animIndex), () => OnCompleted(fade, loop));
        return this;
    }

    public AnimationObject OnComplete(Action complete, float fade = 0.0f)
    {
        AddNode(new ActionNode(-1), () => { complete(); OnCompleted(fade, false); });
        return this;
    }

    private void Animate(int animIndex, float fade, bool loop)
    {
        if (animIndex >= 0 && animIndex < ClipCount)
        {
            SetLoop(animIndex, loop);
            PlayClip(animIndex, fade);
            StartTimer(GetDuration(animIndex) - fade, loop);
            Debug.Log("Animate " + GetName(animIndex) + " " + Time.time);
        }
        else
        {
            StartTimer(fade, loop);
        }
    }

    private void PlayClip(int animIndex, float fade)
    {
        _animation.CrossFade(GetName(animIndex), fade);
    }

    private void OnCompleted(float fade, bool loop)
    {
        if (!MoveNext()) return;
        Animate(_headNode.animIndex, fade, loop);
    }

    private void CreateNode(int animIndex)
    {
        _headNode = _tailNode = new ActionNode(animIndex);
    }

    private void AddNode(ActionNode node, Action action)
    {
        _tailNode.action = action;
        _tailNode.next = node;
        _tailNode = _tailNode.next;
    }

    private bool MoveNext()
    {
        if (_headNode != null)
        {
            _headNode = _headNode.next;
            return true;
        }
        return false;
    }

    private void StartTimer(float duration, bool loop)
    {
        _run = true;
        _loop = loop;
        _time = 0.0f;
        _duration = duration;
    }

    private void StopTimer()
    {
        _run = false;
        _loop = false;
        var index = _headNode.animIndex;
        if (index < 0 || index >= ClipCount) return;
        SetAnimation(index, GetDuration(index));
    }

    private void StopLoop(float fade = 0.0f, bool nextLoop = false)
    {
        _run = false;
        _loop = false;
        var index = _headNode.animIndex;
        if (index >= 0 && index < ClipCount)
            SetAnimation(index, GetDuration(index));
        _headNode.action?.Invoke();
    }

    private void Update()
    {
        #region Test
        if (Input.GetKeyDown(KeyCode.A))
        {
            Play(0).OnComplete(() => Debug.Log("Play 0")).NextPlay(1, true).OnComplete(() => Debug.Log("Play 1")).NextPlay(2).OnComplete(() => Debug.Log("On Complete"));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopLoop();
        }
        #endregion

        if (!_run) return;
        if (_loop || _time < _duration)
        {
            _time += Time.deltaTime;
        }
        else
        {
            _run = false;
            _time = 0.0f;
            _headNode.action?.Invoke();
        }
    }

    #region Utility

    private void Reset()
    {
        _animation = GetComponent<Animation>();
        _animNames = GetNames();
    }

    private string[] GetNames()
    {
        var strList = new List<string>();
        foreach (AnimationState item in _animation)
        {
            strList.Add(item.name);
        }
        return strList.ToArray();
    }

    private string GetName(int animIndex)
    {
        return _animNames[animIndex];
    }

    public AnimationState GetState(int animIndex)
    {
        return _animation[_animNames[animIndex]];
    }

    public AnimationClip GetClip(int animIndex)
    {
        return _animation[_animNames[animIndex]].clip;
    }

    public float GetDuration(int animIndex)
    {
        return _animation[_animNames[animIndex]].length;
    }

    public void SetSpeed(int animIndex, float speed = 1.0f)
    {
        _animation[_animNames[animIndex]].speed = speed;
    }

    public void SetLoop(int animIndex, bool loop = false)
    {
        _animation[_animNames[animIndex]].wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
    }

    public bool IsAnimating(int animIndex = 0)
    {
        return _animation.IsPlaying(_animNames[animIndex]);
    }

    public void SetAnimation(int animIndex = 0, float time = 0.0f)
    {
        _animation.Stop();
        GetClip(animIndex).SampleAnimation(gameObject, time);
    }

    public int ClipCount { get { return _animation.GetClipCount(); } }

    #endregion

    [ContextMenu("Go")]
    public void Go()
    {
        Play(0).OnComplete(() => Debug.Log("Play 0")).NextPlay(1, true).OnComplete(() => Debug.Log("Play 1")).NextPlay(2).OnComplete(() => Debug.Log("On Complete"));
    }

    [ContextMenu("Back")]
    public void Back()
    {
        StopLoop();
    }
}
