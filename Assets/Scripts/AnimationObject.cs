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


    private void Reset()
    {
        _animation = GetComponent<Animation>();
        _animNames = GetAnimationNames();
    }

    public AnimationObject Animate(int animIndex, float fadeTime = 0)
    {
        PlayClip(_tailNode = _actionNode = new AnimNode(animIndex), fadeTime);
        return this;
    }

    public AnimationObject NextPlay(int animIndex, float fadeTime = 0)
    {
        AddNode(new AnimNode(animIndex), () => OnCompleted(fadeTime));
        return this;
    }

    public AnimationObject OnComplete(Action complete)
    {
        AddNode(new AnimNode(-1), complete);
        return this;
    }

    private void AddNode(AnimNode node, Action callback)
    {
        _tailNode.next = node;
        _tailNode = _tailNode.next;
        _actionNode.action = callback;
        _actionNode = _actionNode.next;
    }

    private void PlayClip(AnimNode node, float fadeTime)
    {
        _headNode = node;
        _animation.CrossFade(_animNames[_headNode.index], fadeTime);
        StartCoroutine(RunTimer(GetLength(_headNode.index) - fadeTime));
        Debug.Log("Current Play " + _animNames[_headNode.index] + " Time " + Time.time);
    }

    private void OnCompleted(float fadeTime = 0)
    {
        PlayClip(_headNode.next, fadeTime);
    }

    private IEnumerator RunTimer(float duration)
    {
        yield return new WaitForComplete(duration);
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
        Animate(0).NextPlay(1).NextPlay(2).NextPlay(3).OnComplete(() => Debug.Log("Completed " + Time.time));
    }

    [ContextMenu("FadeGo")]
    public void Fade()
    {
        Animate(0, 1f).NextPlay(1, 1f).NextPlay(2).NextPlay(3).OnComplete(() => Debug.Log("Completed " + Time.time));
    }
}