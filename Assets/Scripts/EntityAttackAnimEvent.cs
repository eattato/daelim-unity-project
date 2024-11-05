using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAttackAnimEvent : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Entity entity = animator.gameObject.GetComponent<Entity>();
        //entity.Movable = false;
        //animator.applyRootMotion = true;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 모션 끝날때 재행동 가능하게 만들어줌
        // 모션 캔슬해야 할 시, any state로 넘어가면 발동 안하고 넘길 수 있음
        // hasExitTime on: 모션이 끝나야만 발동
        // hasExitTime off: 상태전환 이루어지면 바로 발동

        // hasExitTime이 없어 재생 중 나왔거나, Any State으로 이동되어 캔슬된 경우 리턴
        if (stateInfo.normalizedTime % 1 != 1) return;
        Entity entity = animator.gameObject.GetComponent<Entity>();
        entity.EnableAct();
        entity.EnableMove();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
