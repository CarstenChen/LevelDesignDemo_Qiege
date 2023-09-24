using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpatialCrack : MonoBehaviour
{
    public UnityEvent OnDeactive;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeactiveSelf()
    {
        OnDeactive.Invoke();
        this.gameObject.SetActive(false);
    }
}
