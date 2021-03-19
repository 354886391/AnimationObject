using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Grover Gaming/Utility/Framed Animation")]
public class FramedAnimation : MonoBehaviour
{
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public string _propertyName = "_MainTex";
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public int _framesPerSecond = 30;
    private const string UseProperty = "Direct use of this field is not recommended. Use the associated property instead.";
    private const float Epsilon = 0.001f;
    private const string UpdateFrameMethodName = "UpdateFrame";
    private const string RestartLoopMethodName = "RestartLoop";
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public bool _playOnAwake;
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public bool _loop;
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public float _loopDelayMinimum;
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public float _loopDelayMaximum;
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public Renderer _target;
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public int _materialIndex;
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public bool _sharedMaterial;
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public int _startingFrame;
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public bool _randomStartingFrame;
    //[Obsolete("Direct use of this field is not recommended. Use the associated property instead.")]
    public Texture2D[] _frames;
    private int _currentFrame;
    private Material _material;

    public event Predicate<FramedAnimation> Playing;

    public event Action<FramedAnimation> Played;

    public event Action<FramedAnimation> LoopCompleted;

    public event Predicate<FramedAnimation> Completing;

    public event Predicate<FramedAnimation> Stopping;

    public event Predicate<FramedAnimation> Reverting;

    public event Action<FramedAnimation> Reverted;

    public event Action<FramedAnimation, int> FrameChanged;

    public virtual bool PlayOnAwake
    {
        get
        {
            return _playOnAwake;
        }
        set
        {
            _playOnAwake = value;
        }
    }

    public virtual bool Loop
    {
        get
        {
            return _loop;
        }
        set
        {
            _loop = value;
        }
    }

    public virtual float LoopDelayMinimum
    {
        get
        {
            return _loopDelayMinimum;
        }
        set
        {
            _loopDelayMinimum = value;
        }
    }

    public virtual float LoopDelayMaximum
    {
        get
        {
            return _loopDelayMaximum;
        }
        set
        {
            _loopDelayMaximum = value;
        }
    }

    public virtual Renderer Target
    {
        get
        {
            return _target;
        }
        set
        {
            _target = value;
            CacheMaterial();
        }
    }

    public virtual int MaterialIndex
    {
        get
        {
            return _materialIndex;
        }
        set
        {
            _materialIndex = value;
            CacheMaterial();
        }
    }

    public virtual string PropertyName
    {
        get
        {
            return _propertyName;
        }
        set
        {
            _propertyName = value;
            CacheMaterial();
        }
    }

    public virtual bool SharedMaterial
    {
        get
        {
            return _sharedMaterial;
        }
        set
        {
            _sharedMaterial = value;
            CacheMaterial();
        }
    }

    public virtual int FramesPerSecond
    {
        get
        {
            return _framesPerSecond;
        }
        set
        {
            _framesPerSecond = value;
            FrameDisplayTime = 1f / (float)FramesPerSecond;
        }
    }

    public virtual int StartingFrame
    {
        get
        {
            return _startingFrame;
        }
        set
        {
            if (Frames == null || FrameCount == 0)
                return;
            int num = FrameClamped(value);
            if (num == StartingFrame)
                return;
            _startingFrame = num;
            EndingFrame = SubtractFrame(StartingFrame);
            if (!IsPlaying)
                return;
            OnPlayed();
        }
    }

    public virtual bool RandomStartingFrame
    {
        get
        {
            return _randomStartingFrame;
        }
        set
        {
            _randomStartingFrame = value;
        }
    }

    public virtual Texture2D[] Frames
    {
        get
        {
            return _frames;
        }
        set
        {
            Stop();
            _frames = value;
            CacheFramesInfo();
        }
    }

    public virtual bool IsPlaying { get; protected set; }

    public virtual Queue<Action<FramedAnimation>> CompletedQueue { get; set; }

    public virtual Queue<Action<FramedAnimation>> StoppedQueue { get; set; }

    public virtual Material Material
    {
        get
        {
            return (UnityEngine.Object)_material != (UnityEngine.Object)null ? _material : (_material = !SharedMaterial ? Target.materials[MaterialIndex] : Target.sharedMaterials[MaterialIndex]);
        }
        set
        {
            _material = value;
            if (SharedMaterial)
                Target.sharedMaterials[MaterialIndex] = Material;
            else
                Target.materials[MaterialIndex] = Material;
        }
    }

    public virtual int MaxFrame { get; protected set; }

    public virtual int FrameCount { get; protected set; }

    public virtual int CurrentFrame
    {
        get
        {
            return _currentFrame;
        }
        set
        {
            Texture2D frame = Frames[_currentFrame = value];
            if ((UnityEngine.Object)CachedMaterial.GetTexture(PropertyName) != (UnityEngine.Object)frame)
                CachedMaterial.SetTexture(PropertyName, (Texture)frame);
            OnFrameChanged();
        }
    }

    public int EndingFrame { get; protected set; }

    protected virtual int OffsetFrame { get; set; }

    protected virtual int FrameOffset { get; set; }

    protected virtual float FrameDisplayTime { get; set; }

    protected virtual Material CachedMaterial { get; set; }

    public virtual bool Play()
    {
        return PlayBase(null);
    }

    public virtual bool Play(Action<FramedAnimation> onCompleted)
    {
        return PlayBase(() => AddCompletedListener(onCompleted));
    }

    public virtual bool Play(Action<FramedAnimation> onCompleted, Action<FramedAnimation> onStopped)
    {
        return PlayBase(() =>
        {
            AddCompletedListener(onCompleted);
            AddListener(StoppedQueue, onStopped);
        });
    }

    public virtual void Complete()
    {
        if (!OnCompleting())
            return;
        OnComplete();
        OnCompleted();
    }

    public virtual void Stop()
    {
        if (!OnStopping())
            return;
        OnStop();
        OnStopped();
    }

    public virtual void Revert()
    {
        if (!OnReverting())
            return;
        OnRevert();
        OnReverted();
    }

    protected virtual void Awake()
    {
        if ((UnityEngine.Object)Target == (UnityEngine.Object)null)
            Target = GetComponent<Renderer>();
        CacheFramesInfo();
        FrameDisplayTime = 1f / (float)FramesPerSecond;
        EndingFrame = SubtractFrame(StartingFrame);
        CacheMaterial();
        CompletedQueue = new Queue<Action<FramedAnimation>>(3);
        StoppedQueue = new Queue<Action<FramedAnimation>>(3);
        if (!PlayOnAwake)
            return;
        Play();
    }

    protected virtual bool PlayBase(Action beforePlayAction = null)
    {
        if (!OnPlaying()) return false;
        if (beforePlayAction != null) beforePlayAction();
        OnPlay();
        OnPlayed();
        return true;
    }

    protected virtual void AddListener(
      Queue<Action<FramedAnimation>> listenerQueue,
      Action<FramedAnimation> listener)
    {
        if (listener == null)
            return;
        listenerQueue.Enqueue(listener);
    }

    protected virtual void AddCompletedListener(Action<FramedAnimation> listener)
    {
        AddListener(CompletedQueue, listener);
    }

    protected virtual void OnPlay()
    {
        if (Frames == null || FrameCount == 0 || (double)FramesPerSecond <= 0.0)
        {
            IsPlaying = false;
            OnCompleted();
        }
        else
        {
            OnStop();
            if (RandomStartingFrame)
                StartingFrame = UnityEngine.Random.Range(0, FrameCount);
            IsPlaying = true;
            CurrentFrame = StartingFrame;
            if (FrameCount <= 1)
            {
                IsPlaying = false;
                OnCompleted();
            }
            else
            {
                InvokeRepeating("UpdateFrame", FrameDisplayTime, FrameDisplayTime);
                if (!Loop)
                    return;
                OnCompleted();
            }
        }
    }

    protected virtual void UpdateFrame()
    {
        CurrentFrame = AddFrame(CurrentFrame);
        if (Loop)
        {
            if (CurrentFrame != StartingFrame)
                return;
            OnLoopCompleted();
            if ((double)LoopDelayMinimum <= 0.0 && (double)LoopDelayMaximum <= 0.0)
                return;
            CancelInvoke("UpdateFrame");
            Invoke("RestartLoop", 0.0f);
        }
        if (CurrentFrame != EndingFrame)
            return;
        CancelInvoke("UpdateFrame");
        IsPlaying = false;
        OnCompleted();
    }

    protected virtual void RestartLoop()
    {
        InvokeRepeating("UpdateFrame", (double)Math.Abs(LoopDelayMinimum - LoopDelayMaximum) >= 1.0 / 1000.0 ? UnityEngine.Random.Range(LoopDelayMinimum, LoopDelayMaximum) : LoopDelayMinimum, FrameDisplayTime);
    }

    protected virtual void OnComplete()
    {
        OnStop();
        CurrentFrame = MaxFrame;
        OnCompleted();
    }

    protected virtual void OnStop()
    {
        CancelInvoke("UpdateFrame");
        IsPlaying = false;
    }

    protected virtual void OnRevert()
    {
        CurrentFrame = 0;
        FrameOffset = 0;
        OffsetFrame = 0;
    }

    protected virtual void CacheMaterial()
    {
        _material = (Material)null;
        CachedMaterial = Material;
    }

    protected virtual void CacheFramesInfo()
    {
        if (Frames == null || Frames.Length == 0)
        {
            MaxFrame = 0;
            FrameCount = 0;
        }
        else
        {
            MaxFrame = Frames.Length - 1;
            FrameCount = Frames.Length;
            EndingFrame = SubtractFrame(StartingFrame);
        }
    }

    protected virtual int FrameClamped(int targetFrame)
    {
        return Mathf.Clamp(targetFrame, 0, MaxFrame);
    }

    protected virtual int AddFrame(int frame)
    {
        return frame >= MaxFrame ? 0 : frame + 1;
    }

    protected virtual int SubtractFrame(int frame)
    {
        return frame <= 0 ? MaxFrame : frame - 1;
    }

    protected bool OnPlaying()
    {
        return Playing == null || Playing(this);
    }

    protected void OnPlayed()
    {
        if (Played == null)
            return;
        Played(this);
    }

    protected bool OnCompleting()
    {
        return Completing == null || Completing(this);
    }

    protected void OnCompleted()
    {
        while (CompletedQueue.Count > 0)
        {
            Action<FramedAnimation> action = CompletedQueue.Dequeue();
            if (action != null)
                action(this);
        }
    }

    protected void OnLoopCompleted()
    {
        if (LoopCompleted == null)
            return;
        LoopCompleted(this);
    }

    protected bool OnStopping()
    {
        return Stopping == null || Stopping(this);
    }

    protected void OnStopped()
    {
        while (StoppedQueue.Count > 0)
        {
            Action<FramedAnimation> action = StoppedQueue.Dequeue();
            if (action != null)
                action(this);
        }
    }

    protected bool OnReverting()
    {
        return Reverting == null || Reverting(this);
    }

    protected void OnReverted()
    {
        if (Reverted == null)
            return;
        Reverted(this);
    }

    protected void OnFrameChanged()
    {
        if (FrameChanged == null)
            return;
        FrameChanged(this, CurrentFrame);
    }

    [ContextMenu("Play")]
    private void PlayFromMenu()
    {
        Play();
    }

    [ContextMenu("Sort Frames By Name")]
    private void SortFrames()
    {
        Array.Sort<Texture2D>(Frames, (Comparison<Texture2D>)((frame1, frame2) => string.CompareOrdinal(frame1.name, frame2.name)));
    }
}
