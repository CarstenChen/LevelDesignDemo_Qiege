using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulRecover : MonoBehaviour
{
    public bool active;
    public GameObject monster;
    public float goBackSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!active) return;

        GoBackToMonsterBody();
    }

    void GoBackToMonsterBody()
    {
        transform.position = Vector3.MoveTowards(transform.position, monster.transform.position, Time.deltaTime * goBackSpeed);
    }
}
