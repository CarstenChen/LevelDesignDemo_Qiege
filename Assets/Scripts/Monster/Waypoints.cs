using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    public Transform root;
    public Transform[] wayPoints;

    // Start is called before the first frame update
    void Start()
    {
        InitializeWaypoints();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeWaypoints()
    {
        wayPoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            wayPoints[i] = transform.GetChild(i).transform;
        }
    }
}
