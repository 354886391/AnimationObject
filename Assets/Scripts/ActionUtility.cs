﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionUtility : MonoBehaviour
{
    private class Node
    {
        public bool run;
        public float timer;
        public float length;
        public Action action;
        public Node next;

        public Node(float timer, float length, Action action, Node next)
        {
            this.timer = timer;
            this.length = length;
            this.action = action;
            this.next = next;
        }

        /// <summary>
        /// 执行 action 方法 并开启计时器
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void NodeStart()
        {
            if (!run)
            {
                run = true;
                action?.Invoke();
            }
        }

        /// <summary>
        /// 节点计时器, 计时完毕后执行回调函数
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="callback"></param>
        public void NodeTimer(float deltaTime, Action<Node> callback)
        {
            NodeStart();
            if (run)
            {
                if (length < 0) return;
                if (timer < length)
                {
                    timer = Mathf.Min(timer + deltaTime, length);
                }
                else
                {
                    NodeEnd(callback);
                }
            }
        }

        /// <summary>
        /// 执行 callback 方法 并关闭计时器
        /// </summary>
        /// <param name="callback"></param>
        private void NodeEnd(Action<Node> callback)
        {
            run = false;
            timer = 0.0f;
            length = 0.0f;
            callback?.Invoke(this);
        }

    }

    private bool _run;
    private bool _loop;
    private bool _pause;
    private float _runTimer;
    private float _loopTimer;
    private float _pauseTimer;
    private float _length;

    private Node _headNode;
    private Node _tailNode;

    /// <summary>
    /// 添加第一个节点
    /// 启动主线计时器
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public ActionUtility Play(float length, Action action)
    {
        FirstNode(length, action);
        StartTimer();
        return this;
    }

    public ActionUtility Next(float length, Action action)
    {
        AddNode(length, action);
        return this;
    }

    private void FirstNode(float length, Action action)
    {
        _headNode = _tailNode = new Node(0.0f, length, action, null);
    }

    private void AddNode(float length, Action action)
    {
        _tailNode = _tailNode.next = new Node(0.0f, length, action, null);
    }

    private bool MoveNext(Node current)
    {
        if (current.next != null)
        {
            _headNode = current.next;
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

    private void UpdateNode(float deltaTime)
    {
        if (_run && _headNode != null)
        {
            _runTimer += deltaTime;
            _headNode.NodeTimer(deltaTime, current =>
            {
                if (!MoveNext(current))
                {
                    _run = false;
                    _runTimer = 0.0f;
                }
            });

            if (_loop)
            {
                _headNode.length = -1f;
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
        UpdateNode(Time.deltaTime);

        #region Test
        if (Input.GetKeyDown(KeyCode.A))
        {
            // 普通
            PlayTest();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            // 暂停
            PlayTest();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            // 继续
            PlayTest();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            // 终止
            PlayTest();
        }
        ////////////////////////////////
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 循环
            PlayLoopTest();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 停止循环并继续
            PlayTest();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            // 停止循环并终止
            PlayTest();
        }
        #endregion

    }


    [ContextMenu("Test")]
    private void PlayTest()
    {
        Play(1f, () => Debug.Log("01 complete")).Next(2f, () => Debug.Log("02 complete")).Next(3f, () => Debug.Log("03 complete")).Next(0.0f, () => Debug.Log("All Complete"));
    }

    [ContextMenu("Test")]
    private void PlayLoopTest()
    {
        Play(1f, () => Debug.Log("01 complete")).Next(-1f, () => Debug.Log("02 complete")).Next(3f, () => Debug.Log("03 complete")).Next(0.0f, () => Debug.Log("All Complete"));
    }

}
