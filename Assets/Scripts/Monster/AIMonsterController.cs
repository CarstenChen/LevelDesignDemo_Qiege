using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using Gamekit3D;
[Serializable]
public class Parameter
{
    [Header("Target")]
    public Transform chaseTarget;


    [Header("Speed")]
    public float fastDeccelaration = 100f;
    public float normalAcceleration = 10f;
    public float normalChaseAcceleration = 10f;
    public float normalSpeed = 1f;
    public float normalChaseSpeed = 3f;
    public float speedUpRate = 0.2f;
    public float[] coolDown;
    public int dizzyHitTimes = 3;
    public float catchDistance = 2f;
    [NonSerialized] public float currentChaseSpeed;

    [Header("Animation")]
    public Animator animator;
    public AnimatorInfo animatorCache;

    [Header("Mesh Settings")]
    public GameObject bodyMesh;
    public GameObject myJaw;

    [Header("Link Settings")]
    public LinkObjects link;

    [Header("Support")]
    public bool suppprt = false;
    public float supportTime = 5f;
    public float supportInterial = 3f;
}


    public class AIMonsterController : MonoBehaviour
{
    public enum StateType
    {
        Idle, Chase, Attack, Patrol, Dizzy, Death, Link, HelpChase
    }


    [Header("State Info")]
    [ SerializeField]protected State currentState;
    protected bool blockAI;
    protected bool lastPlayerInSight;
    protected bool lastPlayerInSphereTrigger;
    public bool isInDizzyState;
    protected bool isAttacking;
    public bool isDead;
    public AIMonsterController helpTarget;


    [Header("Param")]
    public Parameter param;
    [NonSerialized] public NavMeshAgent agent;

    [Header("Patrol Settings")]
    public Waypoints[] routes;
    [NonSerialized] public Waypoints currentPatrolRoute;

    [Header("State Info")]
    protected Damageable damageInfo;
    public GameObject HPCanvas;

    [Header("Support Info")]
    protected bool shouldSupport;
    protected bool isSupporting;
    protected float supportCountDown;
    protected Coroutine currentSupportCoroutine;
    public HelpLinkController helpLink;
    public GameObject[] toHelp;

    [NonSerialized] public bool readyToChase;
    [NonSerialized] public bool playerInSight;
    [NonSerialized] public bool playerInSphereTrigger;
    [NonSerialized] public bool playerHeard;
    [NonSerialized] public bool playerFirstFound;
    [NonSerialized] public int hitTimes = 0;
    [NonSerialized] public float currentCoolDown;
    [NonSerialized] public int dizzyTimes = 0;
    [NonSerialized] public bool attackOver = true;




    protected Dictionary<StateType, State> states = new Dictionary<StateType, State>();
    //player info

    [Header("Skills")]
    public MeleeWeapon[] meleeWeapons;
    public MagicHand magicHand;

    public void Awake()
    {
        InitializeComponents();
        InitilizeAnimatorCache();



        SetCurrentPatrolRoute(0);
        //previousPatrolRoute = routes[0];
        RegisterState();

        supportCountDown = param.supportTime;
    }

    private void Start()
    {
        SetAgentParam(param.normalSpeed, param.normalAcceleration);

        //以Idle状态进入
        SwitchToState(StateType.Idle);

        foreach (var weapon in meleeWeapons)
        {
            weapon.SetWeaponOwner(gameObject);
        }
        ShowHideMeleeWeapon(false);
    }

    private void Update()
    {
        CacheAnimatorState();
        param.animator.SetBool("PlayerInAttackRange", CalculateDistanceToChaseTarget()<=param.catchDistance );

        if (param.suppprt) UpdateSupport();
    }

    private void FixedUpdate()
    {
        ShowHideMeleeWeapon(isAttacking&&!isDead&&!isInDizzyState);

        //Debug.Log(isAttacking);
        if (GameManager.isGameOver || GameManager.isRespawning ||blockAI) return;
        if (currentCoolDown > 0) currentCoolDown -= Time.fixedDeltaTime;
        else currentCoolDown = 0;

        if (param.suppprt)
            helpTarget = FindMonsterToHelp();

        if (!isInDizzyState && (!param.suppprt||(param.suppprt&&(!shouldSupport|| !helpTarget))))
        {
            if ((currentState.GetType() == typeof(MonsterChaseHelpState) || currentState.GetType() == typeof(MonsterLinkState)))
            {
                SwitchToState(StateType.Idle);
            }

            if (playerInSphereTrigger && CalculateDistanceToChaseTarget() <= param.catchDistance && ((!isAttacking && (currentState.GetType() == typeof(MonsterChaseState))) || (!isAttacking && attackOver && currentState.GetType() == typeof(MonsterAttackState))))
            {

                SwitchToState(StateType.Attack);
            }

            //出范围待机等切换
            if (attackOver && CalculateDistanceToChaseTarget() > param.catchDistance && currentState.GetType() == typeof(MonsterAttackState)&&!param.animatorCache.currentStateInfo.IsName("Attack"))
            {
                SwitchToState(StateType.Idle);
            }

            //巡逻
            if (!playerInSphereTrigger && currentState.GetType() == typeof(MonsterIdleState)&&routes.Length>0)
            {
                SwitchToState(StateType.Patrol);
            }

            if (playerInSphereTrigger && (currentState.GetType() == typeof(MonsterIdleState) || currentState.GetType() == typeof(MonsterPatrolState)))
            {
                SwitchToState(StateType.Chase);
            }

            //出范围待机等切换
            if (!playerInSphereTrigger && !playerFirstFound && hitTimes < param.dizzyHitTimes && currentState.GetType() == typeof(MonsterChaseState))
            {
                SwitchToState(StateType.Idle);
            }
        }
        else if (param.suppprt&& shouldSupport && helpTarget)
        {
            if ((currentState.GetType() == typeof(MonsterIdleState) || currentState.GetType() == typeof(MonsterAttackState) || currentState.GetType() == typeof(MonsterChaseState)) && shouldSupport)
            {
                SwitchToState(StateType.HelpChase);
            }

            if (!isSupporting && helpTarget != null && CalculateDistanceToHelpTarget() <= param.catchDistance && currentState.GetType() == typeof(MonsterChaseHelpState))
            {
                currentSupportCoroutine = StartCoroutine(Support());
                SwitchToState(StateType.Link);
            }

            if (helpTarget == null && (currentState.GetType() == typeof(MonsterChaseHelpState) || currentState.GetType() == typeof(MonsterLinkState)))
            {
                SwitchToState(StateType.Idle);
            }

        }


        currentState.OnStateStay();

        //记录上一帧玩家是否在视野中和球形Trigger中
        lastPlayerInSight = playerInSight;
        lastPlayerInSphereTrigger = playerInSphereTrigger;
    }

    bool IsWeaponAnimationOnPlay()
    {
        bool isWeaponEquipped;

        isWeaponEquipped = param.animatorCache.nextStateInfo.tagHash == Animator.StringToHash("WeaponEquippedAnim") || param.animatorCache.currentStateInfo.tagHash == Animator.StringToHash("WeaponEquippedAnim");

        return isWeaponEquipped;
    }
    public void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        param.animator = GetComponent<Animator>();
        damageInfo = GetComponent<Damageable>();
        helpLink = GetComponentInChildren<HelpLinkController>();
    }

    public void SwitchToDizzyState()
    {
        SwitchToState(StateType.Dizzy);
        isInDizzyState = true;
        param.animator.SetBool("Dizzy", isInDizzyState);
        param.link.StartSoul();
    }

    public void SwitchToDeathState()
    {
        SwitchToState(StateType.Death);
        blockAI = true;
        isDead = true;
        ShowHideMeleeWeapon(false);
        HPCanvas.SetActive(false);
    }

    public float GetCurrentHp()
    {
        float hp = GetComponent<Damageable>().currentHitPoints / GetComponent<Damageable>().maxHitPoints;

        if(hp<=0 || hp >= 1)
        {
            HPCanvas.GetComponentInChildren<CanvasGroup>().alpha = 0f;
        }
        else
        {
            HPCanvas.GetComponentInChildren<CanvasGroup>().alpha = 1f;
        }
        return hp;
    }

    public void EndDizzyState()
    {

        SwitchToState(StateType.Idle);
        isAttacking = false;
        isInDizzyState = false;
        blockAI = false;
        param.animator.SetBool("Dizzy", isInDizzyState);
        param.link.EndSoul();
        damageInfo.ResetDamage();
    }

    private void InitilizeAnimatorCache()
    {
        param.animatorCache = new AnimatorInfo(param.animator);
    }

    private void SetCurrentPatrolRoute(int i)
    {
        if (routes.Length == 0) return;
        currentPatrolRoute = routes[i];
    }

    private void RegisterState()
    {
        states.Add(StateType.Idle, new MonsterIdleState(this));
        states.Add(StateType.Chase, new MonsterChaseState(this));
        states.Add(StateType.Attack, new MonsterAttackState(this));
        states.Add(StateType.Patrol, new MonsterPatrolState(this));
        states.Add(StateType.Dizzy, new MonsterDizzyState(this));
        states.Add(StateType.Death, new MonsterDeathState(this));
        states.Add(StateType.Link, new MonsterLinkState(this));
        states.Add(StateType.HelpChase,new MonsterChaseHelpState(this));
    }
    public void SwitchToState(StateType type)
    {
        if (currentState != null) currentState.OnStateExit();
        currentState = states[type];
        currentState.OnStateEnter();

    }

    private void SetAgentParam(float speed,float acceleration)
    {
        agent.speed = speed;
        agent.acceleration = acceleration;
    }

    void CacheAnimatorState()
    {
        //上一帧动画信息记录
        param.animatorCache.previousCurrentStateInfo = param.animatorCache.currentStateInfo;
        param.animatorCache.previousIsAnimatorTransitioning = param.animatorCache.isAnimatorTransitioning;
        param.animatorCache.previousNextStateInfo = param.animatorCache.nextStateInfo;

        //当前帧动画信息更新
        param.animatorCache.currentStateInfo = param.animator.GetCurrentAnimatorStateInfo(0);
        param.animatorCache.isAnimatorTransitioning = param.animator.IsInTransition(0);
        param.animatorCache.nextStateInfo = param.animator.GetNextAnimatorStateInfo(0);
    }

    float CalculateDistanceToChaseTarget()
    {
        return Vector3.Distance(transform.position, param.chaseTarget.transform.position);
    }
    float CalculateDistanceToHelpTarget()
    {
        return Vector3.Distance(transform.position, helpTarget.transform.position);
    }

    public void MonsterMeleeAttackStart()
    {
        foreach (var weapon in meleeWeapons)
        {
            weapon.BeginAttack();
        }

        isAttacking = true;
    }

    public void MonsterRangeAttackStart()
    {
        magicHand.Attack();

        isAttacking = true;
    }

    // This is called by an animation event when Ellen finishes swinging her staff.
    public void MonsterMeleeAttackEnd()
    {
        foreach (var weapon in meleeWeapons)
        {
            weapon.EndAttack();
        }

        isAttacking = false;
    }

    public void MonsterRangeAttackEnd()
    {
        isAttacking = false;
    }

    void ShowHideMeleeWeapon(bool isShow)
    {
        if (meleeWeapons == null) return;
        foreach (var weapon in meleeWeapons)
        {
            weapon.gameObject.SetActive(isShow);
        }

        if (!isShow)
            param.animator.ResetTrigger("MeleeAttackTrigger");
    }

    void UpdateSupport()
    {
        if(param.suppprt && !shouldSupport && !isSupporting)
        {
            StartCoroutine(SupportCoolDown());
        }
    }
    IEnumerator Support()
    {
        if (helpTarget)
        {
            isSupporting = true;

            yield return new WaitForSeconds(supportCountDown);

            isSupporting = false;
            shouldSupport = false;
            helpTarget = null;
        }
        else
        {
            shouldSupport = false;
            yield return null;
        }
    }

    public void ForceStopSupport()
    {
        if (currentSupportCoroutine != null)
        {
            StopCoroutine(currentSupportCoroutine);
            currentSupportCoroutine = null;
        }

        isSupporting = false;
        shouldSupport = false;
    }

    IEnumerator SupportCoolDown()
    {
        shouldSupport = false;
        yield return new WaitForSeconds(param.supportInterial);

        shouldSupport = true;
    }

    AIMonsterController FindMonsterToHelp()
    {
        GameObject closestMonster = null;

        if (toHelp.Length == 0) return null;

        for (int i = 0; i < toHelp.Length; i++)
        {
            if (toHelp[i].GetComponent<AIMonsterController>().isDead || toHelp[i].GetComponent<AIMonsterController>().isInDizzyState) continue;

            if (closestMonster == null) closestMonster = toHelp[i];

            else if (Vector3.Distance(toHelp[i].transform.position, transform.position) < Vector3.Distance(closestMonster.transform.position, transform.position))
            {
                closestMonster = toHelp[i];
            }
        }

        if (closestMonster)
            return closestMonster.GetComponent<AIMonsterController>();
        else
            return null;
    }

    public void AttackOver()
    {
        attackOver = true;
    }
}
