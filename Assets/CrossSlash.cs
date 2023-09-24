using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossSlash : MonoBehaviour
{
    public float speed = 15f;

    public GameObject print;

    Coroutine CurrentDestroyCoroutine;

    public bool isEnemyBullet;
    public GameObject destroyRoot;
    private MeleeWeapon weapon;
 
    // Start is called before the first frame update
    private void Awake()
    {
        CurrentDestroyCoroutine=StartCoroutine(SelfDestroy(3));
    }
    private void Start()
    {
        weapon = GetComponent<MeleeWeapon>();

        if(weapon) weapon.BeginAttack();
    }

    // Update is called once per frame
    void Update()
    {
        ShootForward();
    }

    void ShootForward()
    {
        transform.localPosition += transform.forward * Time.deltaTime * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "SpatialCrack")
        {
            SwitchSlashToPrint();
            if(print)
                print.transform.position = other.ClosestPoint(other.transform.position);

            SpatialCrack sc = other.GetComponent<SpatialCrack>();
            if (sc) sc.DeactiveSelf();


            StopAllCoroutines();
        }
        else if(other.tag == "MonsterSoul")
        {
            //SwitchSlashToPrint();
            if (print)
                print.transform.position = other.transform.position;
            other.GetComponentInParent<SoulController>().DealthWithMonsterDeath();
        }
        else if (other.tag == "Player" && isEnemyBullet)
        {
            Debug.Log("HitPlayer");
            CurrentDestroyCoroutine = StartCoroutine(SelfDestroy(0));
        }
        
    }

    void SwitchSlashToPrint()
    {
        if (print == null) return;
        GetComponent<ParticleController>().DisableAllParticles();
        print.SetActive(true);
        print.transform.SetParent(GameObject.Find("CrackContainer").transform, true);
    }

    IEnumerator SelfDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        if (weapon) weapon.EndAttack();
        Destroy(destroyRoot==null?this.gameObject: destroyRoot);
    }
}
