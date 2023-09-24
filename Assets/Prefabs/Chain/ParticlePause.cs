using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticlePause : MonoBehaviour
{
	public float waitTime;
    void OnEnable()
    {
		Pause();
	}
	void Pause()
	{
		ParticleSystem particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
			StartCoroutine(StartPause(particleSystem, waitTime));
        }
        else
        {
			Debug.Log("ok");
        }

	}

	IEnumerator StartPause(ParticleSystem ps, float time)
	{
		yield return new WaitForSeconds(time - Time.fixedDeltaTime);
		ps.Pause();
	}
}