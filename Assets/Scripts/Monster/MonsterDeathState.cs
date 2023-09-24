using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Gamekit3D;
public class MonsterDeathState : State
{
    private AIMonsterController monster;
    private NavMeshAgent agent;
    private Parameter param;

    private AIMonsterController target;
    private Damageable d;
    public MonsterDeathState(AIMonsterController monster)
    {
        this.monster = monster;
        this.agent = monster.agent;
        this.param = monster.param;
    }

    public void OnStateEnter()
    {
        Debug.Log("Death");

        agent.acceleration = param.fastDeccelaration;
        agent.isStopped = true;
        agent.enabled = false;

        param.animator.SetBool("Death", true);

        monster.GetComponent<Damageable>().CompleteDeath();

        //target = monster.helpTarget;
        //if (target != null)
        //{
        //    d = target.GetComponent<Damageable>();
        //    if (d)
        //    {
        //        d.isInvulnerable = false;
        //        d.invulnerabiltyTime = 0;
        //    }

        //}
    }
    public void OnStateStay()
    {

    }

    public void OnStateExit()
    {

    }
}