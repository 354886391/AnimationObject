using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionUtility : MonoBehaviour
{
    private class ActionNode
    {
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
    public ActionUtility Play()
    {
        FirstNode();
        return this;
    }

    public ActionUtility Next()
    {
        AddNode();
        return this;
    }

    public ActionUtility Delay()
    {
        return this;
    }

    public ActionUtility OnComplete()
    {
        return this;
    }

    private void FirstNode(float length)
    {
        _headNode = _tailNode = new ActionNode(length, null, null);
    }

    private void AddNode(ActionNode node, Action action)
    {
        _tailNode.action = action;
        _tailNode.next = node;
        _tailNode = _tailNode.next;
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


    private void RunUpdate(float deltaTime)
    {
        if (_run)
        {
            _runTimer += deltaTime;
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

}
