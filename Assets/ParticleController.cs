using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{

    public GameObject[] particles;
    public bool playOnEnable;

    private void OnEnable()
    {
        if (playOnEnable)
        {
            foreach (var p in particles)
            {
                p.SetActive(true);
                p.GetComponent<ParticleSystem>().Play();
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAllParticles() {
        foreach (var p in particles)
        {
                p.GetComponent<ParticleSystem>().Play();
        }
    }

    public void DisableAllParticles()
    {
        foreach (var p in particles)
        {
            p.SetActive(false);
        }
    }
}
