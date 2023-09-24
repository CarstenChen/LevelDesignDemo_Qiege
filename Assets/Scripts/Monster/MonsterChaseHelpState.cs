using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MonsterChaseHelpState : State
{
    private AIMonsterController monster;
    private NavMeshAgent agent;
    private Parameter param;
    private AIMonsterController target;
    public MonsterChaseHelpState(AIMonsterController monster)
    {
        this.monster = monster;
        this.agent = monster.agent;
        this.param = monster.param;
    }

    public void OnStateEnter()
    {
        Debug.Log("HelpChase");
        agent.enabled = true;
        agent.speed = param.normalChaseSpeed;
        agent.acceleration = param.normalChaseAcceleration;
        agent.isStopped = false;
        agent.autoBraking = false;

        target = monster.helpTarget;
        if (target != null)
        {
            agent.destination = target.transform.position;
            param.animator.SetBool("HelpChase", true);
        }
        else
        {
            monster.ForceStopSupport();
        }
    }
    public void OnStateStay()
    {
        param.animator.SetFloat("Speed", agent.speed);

        if(target == null) monster.ForceStopSupport();
        agent.destination = target.transform.position;
    }

    public void OnStateExit()
    {
        monster.readyToChase = false;
        monster.playerInSphereTrigger = false;
        monster.playerHeard = false;
        agent.speed = param.normalSpeed;
        agent.acceleration = param.normalAcceleration;
        agent.autoBraking = true;

        param.animator.SetBool("HelpChase", false);
    }


}