using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleStop : MonoBehaviour
{
    ParticleSystem particle;
    // Start is called before the first frame update
    void OnEnable()
    {
        particle = GetComponent<ParticleSystem>();
        particle.Play();
        StartCoroutine(AutoStop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator AutoStop()
    {
        yield return new WaitForSeconds(0.3f);
        particle.Pause();
    }
}
