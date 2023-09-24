using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Ability : MonoBehaviour
{
    public class AbilityFloatEvent : UnityEvent<float> 
    {
        
    }

    public AbilityFloatEvent OnAbilityCanUseEvent = new AbilityFloatEvent();

    [Header("Ability Info")]
    public float cooldownTime = 1f;
    private bool canUse = true;

    public void TriggerAbility()
    {
        if (canUse)
        {
            OnAbilityCanUseEvent.Invoke(cooldownTime);
            AbilityFunction();
            StartCooldown();
        }
    }

    public abstract void AbilityFunction();

    void StartCooldown()
    {
        StartCoroutine(Cooldown());

        IEnumerator Cooldown()
        {
            canUse = false;
            yield return new WaitForSeconds(cooldownTime);
            canUse = true;
        }
    }
}
