using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorInfo
{
    // Information about the base layer of the animator cached.
    public AnimatorStateInfo currentStateInfo;
    public AnimatorStateInfo nextStateInfo;
    public bool isAnimatorTransitioning;

    // Information about the base layer of the animator from last frame.
    public AnimatorStateInfo previousCurrentStateInfo;
    public AnimatorStateInfo previousNextStateInfo;
    public bool previousIsAnimatorTransitioning;

    public AnimatorInfo(Animator animator)
    {
        currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        nextStateInfo = animator.GetNextAnimatorStateInfo(0);
        isAnimatorTransitioning = animator.IsInTransition(0);
    }
}