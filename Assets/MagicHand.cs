using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicHand : MonoBehaviour
{
    public GameObject magicFlame;
    public GameObject root;

    private void Start()
    {
        
    }

    public void Attack()
    {
        StartCoroutine(ShootMagicFlame());
    }
    IEnumerator ShootMagicFlame()
    {
        yield return new WaitForSeconds(0.01f);

        Vector3 dir = PlayerController.Instance.transform.position - root.transform.position + 1.3f*Vector3.up;

        //GameObject newSlash = Instantiate(bullet, pilvs.position, Quaternion.LookRotation(pilvs.forward));
        GameObject newSlash = Instantiate(magicFlame, root.transform.position, Quaternion.LookRotation(dir));
        newSlash.GetComponent<MeleeWeapon>().SetWeaponOwner(GetComponentInParent<AIMonsterController>().gameObject);
    }
}
