using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cross : MonoBehaviour
{
    public static Cross instance { get; private set; }
    private float x;
    private float y;

    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        x = Mathf.Clamp(0.5f* Screen.width, 0, Screen.width);
        y = Mathf.Clamp(0.5f * Screen.height, 0, Screen.height);
        this.transform.position = new Vector3(x, y, 0);
    }
}
