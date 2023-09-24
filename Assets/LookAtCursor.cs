using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCursor : MonoBehaviour
{
    private RaycastHit mouseRayHit;
    public LayerMask mouseRayMask;
    private Vector3 targetPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Physics.Raycast(Camera.main.ScreenPointToRay(Cross.instance.transform.position), out mouseRayHit, 500, mouseRayMask);

        targetPoint = mouseRayHit.collider ? mouseRayHit.point : Camera.main.ScreenToWorldPoint(Cross.instance.transform.position + Vector3.forward * 500);

        transform.LookAt(targetPoint);
    }
}
