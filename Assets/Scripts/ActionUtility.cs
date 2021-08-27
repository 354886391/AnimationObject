using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionUtility : MonoBehaviour
{
    private class ActionNode
    {
        public float timer;
        public float length;
        public Action action;
        public ActionNode next;

        public ActionNode(float length, Action action, ActionNode next)
        {
            this.length = length;
            this.action = action;
            this.next = next;
        }
    }

    private bool _run;
    private bool _loop;
    private bool _pause;
    private float _runTimer;
    private float _loopTimer;
    private float _pauseTimer;
    private float _length;

    private ActionNode _headNode;
    private ActionNode _tailNode;

    private ActionNode Current { get => _headNode; }

    /// <summary>
    /// 添加第一个节点
    /// 启动主线计时器
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public ActionUtility Play(float length, Action action)
    {
        FirstNode(length, action);
        _headNode.action?.Invoke();
        StartTimer();
        return this;
    }

    public ActionUtility Next(float length, Action action)
    {
        AddNode(length, action);
        return this;
    }

    public ActionUtility OnComplete(Action action)
    {
        AddNode(0.0f, action);
        return this;
    }

    private void FirstNode(float length, Action action)
    {
        _headNode = _tailNode = new ActionNode(length, action, null);
    }

    private void AddNode(float length, Action action)
    {
        _tailNode = _tailNode.next = new ActionNode(length, action, null);
    }

    private bool MoveNext()
    {
        if (_headNode.next != null)
        {
            _headNode = _headNode.next;
            return true;
        }
        return false;
    }

    private void StartTimer()
    {
        _run = true;
        _pause = false;
        _loop = false;
        _runTimer = 0.0f;
    }

    private void PauseTimer()
    {
        _pause = true;
    }

    private void StopTimer()
    {
        _run = false;
    }

    private void RunUpdate(float deltaTime)
    {
        if (_headNode == null) return;
        if (_run)
        {
            _runTimer += deltaTime;
            if (_headNode.timer < _headNode.length)
            {
                _headNode.timer += deltaTime;
            }
            else if (MoveNext())
            {
                _headNode.action?.Invoke();
            }

            if (_loop)
            {

            }

            if (_pause)
            {

            }
        }
    }

    public virtual float GetLength()
    {
        return 0.0f;
    }

    private void Update()
    {
        RunUpdate(Time.deltaTime);
    }


    [ContextMenu("Test")]
    private void Test()
    {
        Play(1f, () => Debug.Log("01 complete")).Next(2f, () => Debug.Log("02 complete")).Next(3f, () => Debug.Log("03 complete")).OnComplete(() => Debug.Log("All Complete"));
    }

}
