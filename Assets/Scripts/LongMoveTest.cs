using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动画暂停重播
/// </summary>
public class LongMoveTest : MonoBehaviour
{
    public Animation Anim;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Anim.Play("LongMove");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Anim["LongMove"].speed = 0;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Anim["LongMove"].speed = 1;
        }
        Debug.Log(Anim["LongMove"].time);
    }

    public void SetAnimation(float time = 0.0f)
    {
        Anim.Stop();
        Anim.clip.SampleAnimation(gameObject, time);
    }
}
