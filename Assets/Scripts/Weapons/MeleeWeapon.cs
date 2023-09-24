using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Gamekit3D;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeleeWeapon : MonoBehaviour
{
    [System.Serializable]
    public class AttackPoint
    {
        public float radius;
        public Vector3 offset;
        public Transform rootTransform;

        [System.NonSerialized] public List<Vector3> previousPos = new List<Vector3>();
    }

    protected GameObject weaponOwner;

    [Header("Weapon Param")]
    public int damage = 1;
    public LayerMask targetLayer;
    public ParticleSystem hitParticle;
    //public TrailEffects[] trailEffects = new TrailEffects[0];

    [Header("Hit Points")]
    public AttackPoint[] attackPoints = new AttackPoint[0];

    protected Vector3[] previousAttackPointPositions;
    protected static RaycastHit[] hitRaycastCache = new RaycastHit[32];
    protected bool isAttacking = false;
    protected List<Collider> targetColliders; //用于避免多次攻击同一对象

    protected const int PARTICLE_COUNT = 2;
    protected ParticleSystem[] pregeneratedParticles = new ParticleSystem[PARTICLE_COUNT];
    protected int particleIndex = 0;

    private void Awake()
    {
        PreGenerateHitParticle();
        InitializeTargetColliders();
    }

    private void PreGenerateHitParticle()
    {
        if (hitParticle == null) return;
        for (int i = 0; i < PARTICLE_COUNT; i++)
        {
            pregeneratedParticles[i] = Instantiate(hitParticle);
            pregeneratedParticles[i].Stop();
        }
    }

    private void InitializeTargetColliders()
    {
        targetColliders = new List<Collider>();
    }
    public void SetWeaponOwner(GameObject owner)
    {
        weaponOwner = owner;
    }

    public void BeginAttack()
    {
        isAttacking = true;
        previousAttackPointPositions = new Vector3[attackPoints.Length];

        for(int i = 0; i < attackPoints.Length; i++)
        {
            Vector3 pos = attackPoints[i].rootTransform.position + attackPoints[i].rootTransform.TransformVector(attackPoints[i].offset);

            previousAttackPointPositions[i] = pos;

            attackPoints[i].previousPos.Clear();
            attackPoints[i].previousPos.Add(previousAttackPointPositions[i]);
        }

        targetColliders.Clear();
    }

    public void EndAttack()
    {
        isAttacking = false;

        for (int i = 0; i < attackPoints.Length; i++)
        {
            attackPoints[i].previousPos.Clear();
        }

        targetColliders.Clear();
    }

    private void FixedUpdate()
    {
        if (isAttacking)
        {
            for (int i = 0; i < attackPoints.Length; i++)
            {
                AttackPoint ap = attackPoints[i];

                //随着身体的变动，每帧两点连线就是攻击方向
                Vector3 pos = ap.rootTransform.position + ap.rootTransform.TransformVector(ap.offset);
                Vector3 hitDir = pos - previousAttackPointPositions[i];

                //确保再近的范围也能有射线打出
                if (hitDir.magnitude < 0.0001f)
                {
                    hitDir = Vector3.forward * 0.0001f;
                }

                Ray hitRay = new Ray(pos, hitDir);
                //把击中点记录在hitRaycastCash中并返回数量
                int contactNum = Physics.SphereCastNonAlloc(hitRay, ap.radius, hitRaycastCache, hitDir.magnitude, ~0, QueryTriggerInteraction.Ignore);

                for (int j = 0; j < contactNum; j++)
                {
                    Collider collider = hitRaycastCache[j].collider;

                    if (targetColliders.Contains(collider)) return;

                    targetColliders.Add(collider);
                    if (collider != null)
                    {
                        //Debug.Log("Hit");
                        CheckDamage(collider, ap);
                    }
                }

                //每一帧都记录先前的位置给这个点
                previousAttackPointPositions[i] = pos;
                ap.previousPos.Add(previousAttackPointPositions[i]);
            }
        }
    }

    void CheckDamage(Collider collider, AttackPoint point)
    {
        Damageable dd = collider.GetComponent<Damageable>();

        if (dd == null) return;

        if (dd == weaponOwner) return;

        if((targetLayer.value&(1<<collider.gameObject.layer)) == 0) return;

        Damageable.DamageMessage data = new Damageable.DamageMessage();
        data.damage = damage;
        data.damager = this;
        data.damageSource = weaponOwner.transform.position;
        data.stopCamera = false;

        dd.ApplyDamage(data);



        PlayHitParticleEffect(point.rootTransform.position);
    }

    void PlayHitParticleEffect(Vector3 position)
    {
        if (hitParticle != null)
        {
            pregeneratedParticles[particleIndex].transform.position = position;
            pregeneratedParticles[particleIndex].time = 0;
            pregeneratedParticles[particleIndex].Play();

            particleIndex = (particleIndex + 1) % PARTICLE_COUNT;

        }

    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < attackPoints.Length; i++)
        {
            AttackPoint ap = attackPoints[i];

            if (ap.rootTransform != null)
            {
                Vector3 pos = ap.rootTransform.position + ap.rootTransform.TransformVector(ap.offset);
                Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
                Gizmos.DrawSphere(pos, ap.radius);
            }

            if (ap.previousPos.Count > 1)
            {
                Handles.DrawAAPolyLine(10, ap.previousPos.ToArray());
            }
        }


    }
#endif
}
