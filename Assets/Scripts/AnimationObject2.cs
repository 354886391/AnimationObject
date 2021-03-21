using System;
using System.Collections;
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


public class AnimationObject2 : MonoBehaviour
{
    [SerializeField]
    private Animation _animation;
    [SerializeField]
    private string[] _animNames;

    private ActionNode _headNode;
    private ActionNode _tailNode;

    public ActionNode Current { get { return _headNode; } }



    public AnimationObject2 Play(int animIndex)
    {
        CreateNode(animIndex);
        Animate(animIndex);
        return this;
    }

    public AnimationObject2 NextPlay(int animIndex)
    {
        AddNode(new ActionNode(animIndex), OnCompleted);
        return this;
    }

    public AnimationObject2 OnComplete(Action complete)
    {
        AddNode(new ActionNode(-1), complete);
        return this;
    }

    private void Animate(int animIndex)
    {
        _animation.Play(GetName(animIndex));
        StartCoroutine(RunTimer(GetDuration(animIndex)));
        Debug.Log("Animate " + GetName(animIndex) + " " + Time.deltaTime);
    }

    private void OnCompleted()
    {
        MoveNext();
        Animate(_headNode.animIndex);
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

    private IEnumerator RunTimer(float duration)
    {
        yield return new WaitForComplete2(duration);
        _headNode.action?.Invoke();
    }

    #region Utility

    private void Reset()
    {
        _animation = GetComponent<Animation>();
        _animNames = GetAnimationNames();
    }

    private string[] GetAnimationNames()
    {
        List<string> strList = new List<string>();
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
        Play(0).NextPlay(1).OnComplete(() => Debug.Log("On Complete"));
    }
}
