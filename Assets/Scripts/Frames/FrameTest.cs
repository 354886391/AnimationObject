using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameTest : MonoBehaviour
{
    public float fps;
    public float speed;
    public int loop;

    public FrameAnimation anim;

    // Start is called before the first frame update
    void Start()
    {
        anim.EventTriggered = AnimEventHandle;
        anim.Play("Run", fps, speed, loop);
    }

    public void AnimEventHandle(FrameClip clip, int index)
    {
        Debug.Log("clip: " + clip + " _currFrame: " + index);
    }
}
