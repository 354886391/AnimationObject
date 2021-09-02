using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSequence : MonoBehaviour
{
    private class Node
    {
        private bool running;
        public float runTimer;
        public int loopTimer;
        public Node nextNode;

        private int loopCount;
        private float runLength;
        private float tempTimer;    // 缓存 runTimer 初始值
        private Action action;

        public Node(float timer, float length, int loopTime, Action action)
        {
            this.runTimer = timer;
            this.runLength = length;
            this.loopCount = loopTime;
            this.action = action;
        }

        /// <summary>
        /// 节点计时器, 计时完毕后执行回调函数
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="callback"></param>
        public void NodeTimer(ref Node node, float deltaTime, Action<Node> callback)
        {
            NodeStart();
            if (running)
            {
                if (runLength < 0) return;
                if (tempTimer >= runLength)
                {
                    // loopCount = -1 则无限循环, 每次循环都会调用NodeStart
                    if (loopCount < 0 || loopTimer < loopCount)
                    {
                        if (loopTimer < loopCount)
                        {
                            loopTimer++;
                        }
                        running = false;
                        NodeStart();
                        return;
                    }
                    NodeEnd(callback);
                    MoveNext(ref node);
                }
                else
                {
                    tempTimer = Mathf.Min(tempTimer + deltaTime, runLength);
                }
            }
        }

        /// <summary>
        /// 执行 action 方法 并开启计时器
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void NodeStart()
        {
            if (!running)
            {
                action?.Invoke();
                running = true;
                tempTimer = runTimer;
            }
        }

        /// <summary>
        /// 执行 callback 方法 并关闭计时器
        /// </summary>
        /// <param name="callback"></param>
        private void NodeEnd(Action<Node> callback)
        {
            callback?.Invoke(this);
            running = false;
            loopTimer = 0;
            runTimer = 0.0f;
        }

        /// <summary>
        /// 移动到下一个节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool MoveNext(ref Node node)
        {
            node = nextNode;
            return nextNode != null;
        }

        public void Pause()
        {
            runLength = -1.0f;
        }

        public void Continue()
        {
            runLength = 0.0f;
            loopCount = 0;
        }
    }

    private float _runTimer;
    private Node _headNode;
    private Node _tailNode;

    /// <summary>
    /// 添加第一个节点
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to">当to为 -1 时停留在当前节点</param>
    /// <param name="loops"></param>
    /// <param name="action">执行当前节点的回调(进入时即执行)</param>
    /// <returns></returns>
    public BaseSequence BasePlay(float from, float to, int loops, Action action)
    {
        FirstNode(from, to, loops, action);
        return this;
    }

    /// <summary>
    /// 添加后继节点 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="loops"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public BaseSequence BaseNext(float from, float to, int loops, Action action)
    {
        AddNode(from, to, loops, action);
        return this;
    }

    private void FirstNode(float from, float to, int loops, Action action)
    {
        _headNode = _tailNode = new Node(from, to, loops, action); //Todo ObjectPool
    }

    private void AddNode(float from, float to, int loops, Action action)
    {
        _tailNode = _tailNode.nextNode = new Node(from, to, loops, action);
    }

    protected void UpdateNode(float deltaTime)
    {
        if (_headNode != null)
        {
            _runTimer += deltaTime;
            _headNode.NodeTimer(ref _headNode, deltaTime, current => { /*Debug.Log("<color=yellow>node complete</color> " + current.length);*/ });
        }
    }

    public void Pause()
    {
        _headNode.Pause();
    }

    public void Continue()
    {
        _headNode.Continue();
    }

    public void Release()
    {
        _headNode = null;
    }
}
