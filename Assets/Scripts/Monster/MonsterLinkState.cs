using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Gamekit3D;
public class MonsterLinkState : State
{
    private AIMonsterController monster;
    private NavMeshAgent agent;
    private Parameter param;
    private AIMonsterController target;

    private Damageable d;
    public MonsterLinkState(AIMonsterController monster)
    {
        this.monster = monster;
        this.agent = monster.agent;
        this.param = monster.param;
    }

    public void OnStateEnter()
    {
        Debug.Log("Link");
        agent.enabled = true;
        agent.acceleration = param.fastDeccelaration;
        agent.speed = 0f;
        agent.isStopped = false;

        target = monster.helpTarget;
        if (target == null) monster.ForceStopSupport();
        else
        {
            d = target.GetComponent<Damageable>();
            if (d)
            {
                d.isInvulnerable = true;
                d.invulnerabiltyTime = param.supportTime;
            }


            if (monster.helpLink)
            {
                monster.helpLink.gameObject.SetActive(true);

                LinkObjects lo = monster.helpLink.GetComponent<LinkObjects>();
                if (lo)
                {
                    lo.targetObject = target.param.myJaw;
                    lo.StartMonsterLink();
                }
            }

            param.animator.SetBool("Link", true);
            
            agent.transform.rotation = Quaternion.LookRotation(target.transform.position - agent.transform.position);
        }
    }

    public void OnStateStay()
    {
        if (target.gameObject == null) monster.ForceStopSupport();
    }

    public void OnStateExit()
    {
        agent.speed = param.normalSpeed;
        agent.acceleration = param.normalAcceleration;

        if (d)
        {
            d.invulnerabiltyTime = 0f;
        }

        if (monster.helpLink) monster.helpLink.gameObject.SetActive(false);

        param.animator.SetBool("Link", false);
        if (param.animatorCache.currentStateInfo.IsName("Link")) { param.animator.Play("Link", 0, 0f); }
    }
}