using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public GameObject rotateAround;
    public float speed;
    public Vector3 startOffset;
    public Vector3 targetOffset;
    public bool y;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (GetComponent<ResetPosition>())
            GetComponent<ResetPosition>().RegisterPositionAndRotation();
        Debug.Log("Enable");
        transform.localPosition += startOffset;
        Debug.Log(transform.localPosition);
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(rotateAround.transform.position + targetOffset, y ? Vector3.up : Vector3.forward, speed * Time.deltaTime);
    }
}
