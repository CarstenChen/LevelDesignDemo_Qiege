using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MonsterPatrolState : State
{
    private AIMonsterController monster;
    private Parameter param;
    private NavMeshAgent agent;

    private Waypoints patrolRoute;
    private int destPointIndex = 0;

    public MonsterPatrolState(AIMonsterController monster)
    {
        this.monster = monster;
        this.param = monster.param;
        this.agent = monster.agent;
    }

    public void OnStateEnter()
    {
        Debug.Log("Patrol");

        agent.enabled = true;
        agent.isStopped = false;
        agent.autoBraking = false;

        if (monster.currentPatrolRoute != null)
        {
            patrolRoute = monster.currentPatrolRoute;
            agent.SetDestination(patrolRoute.wayPoints[destPointIndex].position);
        }

    }
    public void OnStateStay()
    {
        param.animator.SetFloat("Speed", agent.speed);

        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance)
            GoToNextPoint();
    }

    public void OnStateExit()
    {
        agent.autoBraking = true;
    }

    private void GoToNextPoint()
    {
        if (patrolRoute==null || patrolRoute.wayPoints.Length == 0)
            return;

        if (agent.enabled == true)
            agent.destination = patrolRoute.wayPoints[destPointIndex].position;

        destPointIndex = (destPointIndex + 1) % patrolRoute.wayPoints.Length;
    }
}