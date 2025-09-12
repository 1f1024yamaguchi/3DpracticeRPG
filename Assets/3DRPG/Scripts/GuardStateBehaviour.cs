using UnityEngine;

public class GuardStateBehaviour : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,int layerIndex)
    {
        PlayerStatus playerStatus = animator.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.OnGuardStart();
        }
    }

    //このステートから出た時に飛び出される(onstateExit)
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int lyaerIndex)
    {
        PlayerStatus playerStatus = animator.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.OnGuardFinished();
        }
    }
}