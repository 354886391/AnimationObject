using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "2D/FrameClip", fileName = "Default")]
[RequireComponent(typeof(FrameClip))]
public class FrameClip : ScriptableObject
{
    [SerializeField, ReadOnly]
    private float _times;

    public int length;
    public float fps;
    public FrameData[] frames;

    private void OnValidate()
    {
        length = frames.Length;
        _times = length / fps;
    }

    [Serializable]
    public class FrameData
    {
        public Sprite sprite;
        public bool triggerEvent = false;
    }
}
