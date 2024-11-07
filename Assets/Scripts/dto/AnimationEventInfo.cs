using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventInfo
{
    AnimationClip clip;
    AnimationEvent onStart;
    AnimationEvent onEnded;

    public AnimationClip Clip
    {
        get { return clip; }
    }

    public AnimationEvent OnStart
    {
        get { return onStart; }
    }

    public AnimationEvent OnEnded
    {
        get { return onEnded; }
    }

    public AnimationEventInfo(AnimationClip clip, int param)
    {
        this.clip = clip;

        this.onStart = new AnimationEvent();
        this.onStart.time = 0;
        this.onStart.functionName = "OnAnimStart";

        this.onEnded = new AnimationEvent();
        this.onEnded.time = clip.length;
        this.onEnded.functionName = "OnAnimEnded";

        clip.AddEvent(this.onStart);
        clip.AddEvent(this.onEnded);
    }
}
