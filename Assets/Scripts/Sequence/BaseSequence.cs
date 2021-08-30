using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSequence : MonoBehaviour
{
    private class Node
    {
        public bool run;
        public bool loop;
        public float timer;
        public float length;
        public Action action;
        public Node nextNode;

        private float tempTimer;    // 缓存 timer 初始值

        public Node(float timer, float length, bool loop, Action action)
        {
            this.timer = timer;
            this.length = length;
            this.loop = loop;
            this.action = action;
        }

        public Node(float timer, float length, bool loop, Action action, Node next)
        {
            this.timer = timer;
            this.length = length;
            this.loop = loop;
            this.action = action;
            this.nextNode = next;
        }

        /// <summary>
        /// 节点计时器, 计时完毕后执行回调函数
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="callback"></param>
        public void NodeTimer(ref Node node, float deltaTime, Action<Node> callback)
        {
            NodeStart();
            if (run)
            {
                if (length < 0) return;
                if (tempTimer >= length)
                {
                    if (loop)
                    {
                        run = false;
                        NodeStart();
                        return;
                    }
                    NodeEnd(callback);
                    MoveNext(ref node);
                }
                else
                {
                    tempTimer = Mathf.Min(tempTimer + deltaTime, length);
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
            if (!run)
            {
                action?.Invoke();
                run = true;
                tempTimer = timer;
            }
        }

        /// <summary>
        /// 执行 callback 方法 并关闭计时器
        /// </summary>
        /// <param name="callback"></param>
        private void NodeEnd(Action<Node> callback)
        {
            callback?.Invoke(this);
            run = false;
            loop = false;
            timer = 0.0f;
            length = 0.0f;
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
    }

    private float _runTimer;
    private Node _headNode;
    private Node _tailNode;

    /// <summary>
    /// 添加第一个节点
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to">当to为 -1 时停留在当前节点</param>
    /// <param name="loop"></param>
    /// <param name="action">执行当前节点的回调(进入时即执行)</param>
    /// <returns></returns>
    public BaseSequence BasePlay(float from, float to, bool loop, Action action)
    {
        FirstNode(from, to, loop, action);
        return this;
    }

    /// <summary>
    /// 添加后继节点 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="loop"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public BaseSequence BaseNext(float from, float to, bool loop, Action action)
    {
        AddNode(from, to, loop, action);
        return this;
    }

    private void FirstNode(float from, float to, bool loop, Action action)
    {
        _headNode = _tailNode = new Node(from, to, loop, action); //Todo ObjectPool
    }

    private void AddNode(float from, float to, bool loop, Action action)
    {
        _tailNode = _tailNode.nextNode = new Node(from, to, loop, action);
    }

    protected void UpdateNode(float deltaTime)
    {
        if (_headNode != null)
        {
            _runTimer += deltaTime;
            _headNode.NodeTimer(ref _headNode, deltaTime, current => { /*Debug.Log("<color=yellow>node complete</color> " + current.length);*/ });
        }
    }

    public void Continue()
    {
        _headNode.loop = false;
        _headNode.length = 0.0f;
    }

    public void Release()
    {
        _headNode = null;
    }
}
