using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
public class MonsterDizzyState : State
{
    private AIMonsterController monster;
    private NavMeshAgent agent;
    private Parameter param;
    public MonsterDizzyState(AIMonsterController monster)
    {
        this.monster = monster;
        this.agent = monster.agent;
        this.param = monster.param;
    }

    public void OnStateEnter()
    {
        agent.enabled = false ;
        GameManager.Instance.firstDizzy = true;
    }
    public void OnStateStay()
    {

    }



    public void OnStateExit()
    {
        agent.enabled = true;
    }
}