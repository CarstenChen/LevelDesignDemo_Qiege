using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MonsterAttackState : State
{
    private AIMonsterController monster;
    private NavMeshAgent agent;
    private Parameter param;
    public MonsterAttackState(AIMonsterController monster)
    {
        this.monster = monster;
        this.agent = monster.agent;
        this.param = monster.param;
    }

    public void OnStateEnter()
    {
        //Debug.Log("Attack");

        agent.enabled = true;
        agent.acceleration = param.fastDeccelaration;
        agent.speed = 0f;

        agent.isStopped = false;

        param.animator.SetTrigger("Attack");

        agent.transform.rotation = Quaternion.LookRotation(param.chaseTarget.transform.position - agent.transform.position);

        monster.attackOver = false;
    }
    public void OnStateStay()
    {

    }

    public void OnStateExit()
    {
        monster.attackOver = false;
        param.animator.ResetTrigger("Attack");
        if (param.animatorCache.currentStateInfo.IsName("Attack")) { param.animator.Play("Attack", 0, 0f); }

        agent.speed = param.normalSpeed;
        agent.acceleration = param.normalAcceleration;
    }
}