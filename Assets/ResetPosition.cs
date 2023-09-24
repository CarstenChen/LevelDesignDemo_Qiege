using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    public Vector3 originalPos;
    public Quaternion originalRotation;
    private void Awake()
    {

    }

    private void ResetPositionAndRotation()
    {
        transform.localPosition = originalPos;
        transform.localRotation = originalRotation;
        Debug.Log("Reset");
    }

    public void RegisterPositionAndRotation()
    {
        originalPos = transform.localPosition;
        originalRotation = transform.localRotation;
        Debug.Log("Reset");
    }
    private void OnDisable()
    {
        ResetPositionAndRotation();
    }
}
