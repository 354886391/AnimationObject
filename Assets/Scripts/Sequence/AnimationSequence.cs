using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSequence : BaseSequence
{

    [SerializeField]
    private Animation _animation;
    [SerializeField]
    private List<string> _clipNames;

    public AnimationSequence Play(float length, Action action)
    {
        BasePlay(0.0f, length, 0, action);
        return this;
    }

    /// <param name="loops"> -1 则无限循环</param>
    public AnimationSequence Play(int index, float speed = 1.0f, int loops = 0)
    {
        BasePlay(0.0f, GetLength(index, speed), loops, () => Animate(index, speed, loops));
        return this;
    }

    /// <summary>
    /// 先执行再延时
    /// </summary>
    /// <param name="length"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public AnimationSequence Before(float delay, Action action)
    {
        BaseNext(0.0f, delay, 0, action);
        return this;
    }

    /// <summary>
    /// 执行回调
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public AnimationSequence Append(Action action)
    {
        BaseNext(0.0f, 0.0f, 0, action);
        return this;
    }

    /// <summary>
    /// 先延时再执行
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public AnimationSequence After(float delay, Action action)
    {
        BaseNext(0.0f, delay, 0, null).BaseNext(0.0f, 0.0f, 0, action);
        return this;
    }

    /// <param name="loops"> -1 则无限循环</param>
    public AnimationSequence Next(int index, float speed = 1.0f, int loops = 0)
    {
        BaseNext(0.0f, GetLength(index, speed), loops, () => Animate(index, speed, loops));
        return this;
    }

    private void Update()
    {
        UpdateNode(Time.deltaTime);
        UpdateTest();
    }

    #region Utility
    private void Reset()
    {
        _animation = GetComponent<Animation>();
        _clipNames = GetNames();
    }

    private List<string> GetNames()
    {
        var strList = new List<string>();
        foreach (AnimationState item in _animation)
        {
            strList.Add(item.name);
        }
        return strList;
    }

    private void Animate(int animIndex, float speed, int loops)
    {
        var state = GetState(animIndex);
        state.speed = speed;
        state.wrapMode = WrapMode.Once;
        _animation.Play(state.clip.name);
    }

    private string GetName(int animIndex)
    {
        return _clipNames[animIndex];
    }

    private AnimationState GetState(int animIndex)
    {
        return _animation[_clipNames[animIndex]];
    }

    private AnimationClip GetClip(int animIndex)
    {
        return _animation[_clipNames[animIndex]].clip;
    }

    private float GetLength(int animIndex, float speed)
    {
        return _animation[_clipNames[animIndex]].length / speed;
    }

    private void SetSpeed(int animIndex, float speed = 1.0f)
    {
        _animation[_clipNames[animIndex]].speed = speed;
    }

    private void SetLoop(int animIndex, bool loop = false)
    {
        _animation[_clipNames[animIndex]].wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
    }

    private bool IsAnimating(int animIndex = 0)
    {
        return _animation.IsPlaying(_clipNames[animIndex]);
    }

    private void SetAnimation(int animIndex = 0, float time = 0.0f)
    {
        _animation.Stop();
        GetClip(animIndex).SampleAnimation(gameObject, time);
    }

    private int GetCount()
    {
        return _animation.GetClipCount();
    }
    #endregion

    private void UpdateTest()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            // 普通 A
            PlayTest();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            // 暂停 B
            PlayPauseTest();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            // 循环 C
            PlayLoopTest();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            // 继续 D
            Continue();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 终止 E
            Release();
        }
    }

    [ContextMenu("Test1")]
    private void PlayTest()
    {
        Play(0)
            .Next(1)
            .Before(1f, () => Debug.Log("before 1 second"))
            .Append(() => Debug.Log("before completed"))
            .Next(2)
            .Append(() => Debug.Log("after 1 second completed"))
            .After(1f, () => Debug.Log("after completed"))
            .Next(3)
            ;
    }

    [ContextMenu("Test2")]
    private void PlayPauseTest()
    {
        Play(0)
            .Next(1)
            .Next(2, 2.0f)
            .Next(3, 2.0f)
            ;
    }

    [ContextMenu("Test3")]
    private void PlayLoopTest()
    {
        Play(0)
            .Next(1)
            .Next(2, 1.0f, -1)
            .Next(3)
            ;
    }
}
