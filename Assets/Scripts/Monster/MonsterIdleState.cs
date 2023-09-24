using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MonsterIdleState : State
{
    private AIMonsterController monster;
    private NavMeshAgent agent;
    private Parameter param;

    public MonsterIdleState(AIMonsterController monster)
    {
        this.monster = monster;
        this.agent = monster.agent;
        this.param = monster.param;
    }


    public void OnStateEnter()
    {
        //Debug.Log("Idle");

        agent.enabled = true;
        agent.speed = 0f;
        agent.isStopped = true;
    }
    public void OnStateStay()
    {
        param.animator.SetFloat("Speed", agent.speed);
    }

    public void OnStateExit()
    {
        agent.speed = param.normalSpeed;
        agent.isStopped = false;
    }
}