﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerSequence : BaseSequence
{

    public TimerSequence Play(float length, Action action)
    {
        BasePlay(0.0f, length, 0, action);
        return this;
    }

    public TimerSequence Play(float length, int loops, Action action)
    {
        BasePlay(0.0f, length, loops, action);
        return this;
    }

    public TimerSequence Next(float length, Action action)
    {
        BaseNext(0.0f, length, 0, action);
        return this;
    }

    public TimerSequence Next(float length, int loops, Action action)
    {
        BaseNext(0.0f, length, loops, action);
        return this;
    }

    private void Update()
    {
        UpdateNode(Time.deltaTime);
        UpdateTest();
    }

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
        Play(1f, () => Debug.Log("01 normal"))
            .Next(2f, () => Debug.Log("02 normal"))
            .Next(3f, () => Debug.Log("03 normal"))
            .Next(0.0f, () => Debug.Log("All complete"))
            ;
    }

    [ContextMenu("Test2")]
    private void PlayPauseTest()
    {
        Play(1f, () => Debug.Log("01 pasue"))
            .Next(-1f, () => Debug.Log("02 pasue"))
            .Next(3f, () => Debug.Log("03 pasue"))
            .Next(0.0f, () => Debug.Log("All complete"))
            ;
    }

    [ContextMenu("Test3")]
    private void PlayLoopTest()
    {
        Play(1f, () => Debug.Log("01 loops"))
            .Next(2f, () => Debug.Log("02 loops"))
            .Next(3f, -1, () => Debug.Log("03 loops"))
            .Next(0.0f, () => Debug.Log("All complete"))
            ;
    }
}
