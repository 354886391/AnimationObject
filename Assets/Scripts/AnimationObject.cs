using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimNode
{
    public int index;
    public Action action;
    public AnimNode next;

    public AnimNode(int index)
    {
        this.index = index;
    }
}

[RequireComponent(typeof(Animation))]
public class AnimationObject : MonoBehaviour
{
    [SerializeField]
    private Animation _animation;
    [SerializeField]
    private string[] _animNames;

    private AnimNode _headNode;
    private AnimNode _tailNode;
    private AnimNode _actionNode;
    private Coroutine _runTimeCoroutine;
    private WaitForComplete _waitComplete;


    private void Reset()
    {
        _animation = GetComponent<Animation>();
        _animNames = GetAnimationNames();
    }

    public AnimationObject Animate(int animIndex, float fadeTime = 0)
    {
        Animate(animIndex, fadeTime, false);
        return this;
    }

    public AnimationObject Animate(int animIndex, bool loop)
    {
        Animate(animIndex, 0.0f, loop);
        return this;
    }

    public AnimationObject Animate(int animIndex, float fadeTime, bool loop)
    {
        PlayClip(_tailNode = _actionNode = new AnimNode(animIndex), fadeTime, loop);
        return this;
    }

    public AnimationObject NextPlay(int animIndex, float fadeTime = 0)
    {
        NextPlay(animIndex, fadeTime, false);
        return this;
    }

    public AnimationObject NextPlay(int animIndex, float fadeTime, bool loop)
    {
        AddNode(new AnimNode(animIndex), () => OnCompleted(fadeTime, loop));
        return this;
    }

    public void StopLoop(int animIndex, bool complete)
    {
        SetAnimation(animIndex, complete ? GetLength(animIndex) : 0.0f);
        if (_waitComplete != null) _waitComplete.Loop = false;
    }

    public AnimationObject OnComplete(Action complete, float delay = 0)
    {
        if (complete == null) return this;
        AddNode(new AnimNode(-1), () => { OnCompleted(delay, false); complete(); });
        return this;
    }

    private void AddNode(AnimNode node, Action callback)
    {
        _tailNode.next = node;
        _tailNode = _tailNode.next;
        _actionNode.action = callback;
        _actionNode = _actionNode.next;
    }

    private void PlayClip(AnimNode node, float fadeTime, bool loop)
    {
        _headNode = node;
        if (_headNode != null && _headNode.index >= 0)
        {
            if (loop) GetState(_headNode.index).wrapMode = WrapMode.Loop;
            _animation.CrossFade(_animNames[_headNode.index], fadeTime);
            StartRuntimer(GetLength(_headNode.index) - fadeTime, loop);
            Debug.Log("Current Play " + _animNames[_headNode.index] + " Time " + Time.time);
        }
        else
        {
            StartRuntimer(fadeTime, loop);
        }
    }

    private void OnCompleted(float fadeTime, bool loop)
    {
        PlayClip(_headNode.next, fadeTime, loop);
    }

    private void StartRuntimer(float duration, bool loop)
    {
        StopRunTimer();
        _runTimeCoroutine = StartCoroutine(RunTimer(duration, loop));
    }

    private void StopRunTimer()
    {
        if (_runTimeCoroutine != null)
        {
            StopCoroutine(_runTimeCoroutine);
            _runTimeCoroutine = null;
        }
    }

    private IEnumerator RunTimer(float duration, bool loop)
    {
        _waitComplete = new WaitForComplete(duration, loop);
        yield return _waitComplete;
        _headNode.action?.Invoke();
    }

    #region Utility
    private string[] GetAnimationNames()
    {
        List<string> strList = new List<string>();
        foreach (AnimationState item in _animation)
        {
            strList.Add(item.name);
        }
        return strList.ToArray();
    }

    public AnimationState GetState(int animIndex)
    {
        return _animation[_animNames[animIndex]];
    }

    public AnimationClip GetClip(int animIndex)
    {
        return _animation[_animNames[animIndex]].clip;
    }

    public float GetLength(int animIndex)
    {
        return _animation[_animNames[animIndex]].length;
    }

    public void SetSpeed(int animIndex, float speed = 1.0f)
    {
        _animation[_animNames[animIndex]].speed = speed;
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
    #endregion

    [ContextMenu("Go")]
    public void Go()
    {
        Animate(0).NextPlay(1).OnComplete(() => Debug.Log("Completed " + Time.time), 1f).NextPlay(2).NextPlay(3).OnComplete(() => Debug.Log("Completed " + Time.time));
    }

    [ContextMenu("FadeGo")]
    public void Fade()
    {
        Animate(0, 1f).NextPlay(1, 1f).NextPlay(2).NextPlay(3).OnComplete(() => Debug.Log("Completed " + Time.time));
    }
}