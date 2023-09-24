using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpRecoverPoint : InteractItem
{
    public float recoverHp = 50f;
    // Start is called before the first frame update
    public override void Interact()
    {
        base.Interact();

        PlayerController.Instance.ApplySelfDamage(-recoverHp, false);
    }
}
