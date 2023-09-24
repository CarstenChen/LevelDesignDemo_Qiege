using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MonsterChaseState : State
{
    private AIMonsterController monster;
    private NavMeshAgent agent;
    private Parameter param;


    protected float tick;
    public MonsterChaseState(AIMonsterController monster)
    {
        this.monster = monster;
        this.agent = monster.agent;
        this.param = monster.param;
    }
    public void OnStateEnter()
    {
        //Debug.Log("Chase");

        agent.enabled = true;
        agent.speed = param.normalChaseSpeed;
        agent.acceleration = param.normalChaseAcceleration;
       agent.isStopped = false;
        agent.autoBraking = false;

        agent.destination = param.chaseTarget.position;
    }
    public void OnStateStay()
    {
        param.animator.SetFloat("Speed", agent.speed);

        agent.destination = param.chaseTarget.position;

    }
    public void OnStateExit()
    {
        monster.readyToChase = false;
        monster.playerInSphereTrigger = false;
        monster.playerHeard = false;
        agent.speed = param.normalSpeed;
        agent.acceleration = param.normalAcceleration;
        agent.autoBraking = true;
    }

}


