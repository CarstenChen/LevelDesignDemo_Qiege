using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : InteractItem
{
    [Header("Respawn Point")]
    public Transform respawnPoint;
    public override void Interact()
    {
        base.Interact();

        PlayerController.Instance.RegisterRespawnPos(respawnPoint.position);
    }
}
