using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    Animator animator;
    List<AnimationClip> clipList;
    Dictionary<string, Action> startActions;
    Dictionary<string, Action> endedActions;
    AnimationEvent lastPlayedEvent = null;

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        clipList = new List<AnimationClip>();
        startActions = new Dictionary<string, Action>();
        endedActions = new Dictionary<string, Action>();

        // 모든 클립에 시작/종료 애니메이션 이벤트 추가
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            AnimationEvent onStart = new AnimationEvent();
            onStart.time = 0;
            onStart.functionName = "OnAnimStart";

            AnimationEvent onEnded = new AnimationEvent();
            onEnded.time = clip.length;
            onEnded.functionName = "OnAnimEnded";

            clip.AddEvent(onStart);
            clip.AddEvent(onEnded);
            clipList.Add(clip);
        }
    }

    public string FindTag(AnimatorStateInfo info, Dictionary<string, Action> actions)
    {
        foreach (string tag in actions.Keys)
        {
            if (info.IsTag(tag))
            {
                return tag;
            }
        }

        return null;
    }

    public void AddStartAction(string tag, Action action)
    {
        startActions.Add(tag, action);
    }

    public void AddEndedAction(string tag, Action action)
    {
        endedActions.Add(tag, action);
    }

    // animEvent에 실행된 애니메이션 이벤트가 들어감
    public void OnAnimStart(AnimationEvent animEvent)
    {
        lastPlayedEvent = animEvent;

        string tag = FindTag(animEvent.animatorStateInfo, startActions);
        if (tag == null) return;

        Action action = startActions[tag];
    }

    public void OnAnimEnded(AnimationEvent animEvent)
    {
        // 마지막으로 재생된 state와 이름이 다르다면, 다른 모션으로 넘어갔지만 트랜지션 때문에 마저 발동된것
        bool cancelled = lastPlayedEvent.animatorStateInfo.fullPathHash != animEvent.animatorStateInfo.fullPathHash;
        if (cancelled) return;

        string tag = FindTag(animEvent.animatorStateInfo, endedActions);
        if (tag == null) return;

        Action action = endedActions[tag];
    }
}
