using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;


public class CombatController : MonoBehaviour
{
    int deathNum;
    int shouldDead;
    bool finish;
    public MonsterGroup[] monsterGroups;
    public GameObject[] activeOnEnd;
    private int nextGroupIndex;

    public UnityEvent OnCombatOver;
    // Start is called before the first frame update
    private bool killAll;
    private int currentGroupIndex;

    private void Start()
    {
        nextGroupIndex = 0;
        currentGroupIndex = 0;

        DealWithBeginningSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (deathNum >= shouldDead)
        {
            currentGroupIndex++;

            if (nextGroupIndex >= monsterGroups.Length && currentGroupIndex >= monsterGroups.Length)
            {
                finish = true;
                killAll = true;
                OnCombatOver.Invoke();

                if (activeOnEnd.Length>0)
                {
                    for (int i = 0; i < activeOnEnd.Length; i++)
                    {
                        activeOnEnd[i].SetActive(true);
                    }

                }
            }

            DealWithLastCallSpawn();
        }
    }

    public void AddDeath()
    {
        deathNum++;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (killAll) return;

        if (other.tag == "Player")
        {
            DealWithTriggerSpawn();
        }

    }

    public void DealWithBeginningSpawn()
    {
        if (!finish && monsterGroups[nextGroupIndex].spawnType == SpawnType.begining && nextGroupIndex< monsterGroups.Length)
        {
            monsterGroups[nextGroupIndex].ActiveMonsterMembers();
            UpdateData();
        }

    }

    public void DealWithLastCallSpawn()
    {
        if(!finish && monsterGroups[nextGroupIndex].spawnType == SpawnType.lastCall && nextGroupIndex < monsterGroups.Length)
        {
            monsterGroups[nextGroupIndex].ActiveMonsterMembers();
            UpdateData();
        }

    }

    public void DealWithTriggerSpawn()
    {
        if (!finish && monsterGroups[nextGroupIndex].spawnType == SpawnType.trigger && nextGroupIndex < monsterGroups.Length)
        {
            monsterGroups[nextGroupIndex].ActiveMonsterMembers();
            UpdateData();
        }

    }
    public void UpdateData()
    {
        deathNum = 0;
        shouldDead = monsterGroups[nextGroupIndex].monsterMembers.Length;

        if (nextGroupIndex< monsterGroups.Length)
        {
            nextGroupIndex++;
        }

    }
}
