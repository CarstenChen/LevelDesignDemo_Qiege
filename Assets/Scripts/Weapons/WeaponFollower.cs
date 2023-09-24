using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//最后执行此类
[DefaultExecutionOrder(9999)]
public class WeaponFollower : MonoBehaviour
{
    [SerializeField] Transform toFollower;
    [SerializeField] Transform toRotateFollower;
    [SerializeField] bool reverse;
    private void FixedUpdate()
    {
        if (!reverse)
        {
            transform.position = toFollower.position;
            transform.rotation = toRotateFollower.rotation;
        }
        else
        {
            transform.position = toFollower.position;
            transform.rotation = toRotateFollower.rotation*Quaternion.AngleAxis(180,Vector3.left);
        }

    }
}
