using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCanvasRotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 v = PlayerController.Instance.transform.position - Camera.main.transform.position;
        transform.rotation = Quaternion.LookRotation(v);
    }
}
