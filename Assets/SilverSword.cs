using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilverSword : Ability
{
    public GameObject bullet;
    public Transform pilvs;
    public Camera mainCamera;

    public override void AbilityFunction()
    {
        Debug.Log("SilverSword");

        StartCoroutine(ShootSilverSlash());

    }

    IEnumerator ShootSilverSlash()
    {
        yield return new WaitForSeconds(0.01f);

        //GameObject newSlash = Instantiate(bullet, pilvs.position, Quaternion.LookRotation(pilvs.forward));
        GameObject newSlash = Instantiate(bullet, mainCamera.transform.position, Quaternion.LookRotation(mainCamera.transform.forward));
    }
}
