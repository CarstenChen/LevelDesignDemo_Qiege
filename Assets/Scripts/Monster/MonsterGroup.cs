using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SpawnType
{
    lastCall, begining, trigger
}
[Serializable]
public class MonsterGroup : MonoBehaviour
{
    public AIMonsterController[] monsterMembers;
    public bool hasSpawn;
    public SpawnType spawnType;

    public void ActiveMonsterMembers()
    {
        foreach (var monster in monsterMembers)
        {
            monster.gameObject.SetActive(true);
        }
        hasSpawn = true;
    }
}
