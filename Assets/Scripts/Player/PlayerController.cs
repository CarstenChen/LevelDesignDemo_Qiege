using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Gamekit3D;
using Gamekit3D.Message;
using DG.Tweening;

public class PlayerController : MonoBehaviour, IMessageReceiver
{
    private static PlayerController instance;
    public static PlayerController Instance { get { return instance; } private set {; } }

    [Header("Camera")]
    [SerializeField] protected CinemachineFreeLook TPSCamera;
    [SerializeField] protected CinemachineVirtualCamera aimCamera;
    [SerializeField] protected Transform headTarget;




   [Header ("Basic Movement")]
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float fastRunSpeed;
    [SerializeField] protected float jumpForwardSpeed;
    [SerializeField] protected float maxRotateSpeed;
    [SerializeField] protected float gravity = 10f;
    [SerializeField] protected float initialJumpSpeed = 10f;

    [SerializeField] protected bool isGrounded = true;
    [SerializeField] protected bool isReadyToJump;
    [SerializeField] protected bool isFastRunning;

    [SerializeField] protected float currentSpeed;
    protected float currentGravity;
    protected float maxSpeedRef = 1f;  //虚拟运动进程区间最大值
    protected float targetSpeedRef;  //实际运动进程区间（需要插值到的）值
    protected float curSpeedRef;  //当前运动进程区间值
    protected float groundAcceleration = 4f;
    protected float groundDeceleration = 5f;
    protected Quaternion targetRotation;  //角色forward转向输入方向的四元数
    protected float shortestDeltaRotDegree;  //当前旋转的delta角（最小角度）
    protected float curRotateSpeed;  //当前速度下的旋转速度
    [SerializeField] protected float curVerticalSpeed; //当前垂直速度速度

    [Header("Step Solution")]
    [SerializeField] protected Transform lowStepChecker;
    [SerializeField] protected Transform highStepChecker;
    [SerializeField] protected float stepSmooth;
    [SerializeField] protected LayerMask stepLayer;
    [SerializeField] protected float stepHeight;

    [Header("Stemina")]
    [SerializeField] protected float maxStemina = 100;
    [SerializeField] protected float steminaCostForFasetRun;
    [SerializeField] protected float steminaRecoverRate;
    [SerializeField] protected float currentStemina;

    [Header("Animation")]
    protected Animator animator;
    protected float idleTimeout = 5f;  //多久开始考虑idle动画
    protected float idleTimer;
    protected AnimatorInfo animatorCache;

    [Header("Input")]
    protected PlayerInput playerInput;
    protected CharacterController character;

    [Header("Skills")]
    public MeleeWeapon[] meleeWeapons;
    public bool isRespawning;
    public bool inNarrative;
    public bool waitToRespawn;
    public bool isCostingStemina;
    public bool isRecoveringStemina;
    protected bool isAttacking;
    private bool fastRunBlock;
    public bool isAiming;
    public Ability SilverSword;

    [Header("Life")]
    public Vector3 toRespawnAt;
    protected Damageable damageable;

    [Header("Gliding")]
    [SerializeField] protected float glideSpeed;
    [SerializeField] protected float glideGravity;
    [SerializeField] protected GameObject glider;
    [SerializeField] protected bool isGliding;
    [SerializeField] protected bool isReadyToGlid;

    [Header("Sea of Chaji")]
    [SerializeField] protected float moveSpeedInWater;
    [SerializeField] protected LayerMask waterLayer;
    [SerializeField] bool isInWater;
    [SerializeField] float waterDamage;
    [SerializeField] float waterOffset;

    void Start()
    {
        //单件
        if (instance == null) instance = this;

        currentSpeed = moveSpeed;

        InitialzePlayerComponent();
        InitializeAnimator();
        InitilizeDamageable();

        ResetStemina();
        ForceToGetIsGrounded(Vector3.forward);
        highStepChecker.transform.localPosition = new Vector3(highStepChecker.transform.localPosition.x, stepHeight, highStepChecker.transform.localPosition.z);

        foreach(var weapon in meleeWeapons)
        {
            weapon.SetWeaponOwner(gameObject);
        }
        ShowHideMeleeWeapon(false);
    }

    private void Update()
    {
        if (InWater())
        {
            
            isInWater = true;
            animator.speed = 0.5f;
            ApplySelfDamage(waterDamage,true);
        }
        else
        {
            isInWater = false;
            animator.speed = 1f;
        }
    }
    void FixedUpdate()
    {
        CacheAnimatorState();
        UpdateInputBlock();

        SetFastRunState(playerInput.FastRunInput && !fastRunBlock);

        ShowHideMeleeWeapon(IsWeaponAnimationOnPlay());
        DealWithMeleeAttackAnimation();


        //移动
        CalculateHorizontalMovement();
        CalculateVerticalMovement();
        CalculateRotation();

        if (isAiming) UpdateAimRotation();
        else if (IsPressingMoveKey)
        {
            CharacterRotate();
        }

        animator.SetFloat("ForwardSpeed", curSpeedRef);
        animator.SetBool("IsFastRunning", isFastRunning);
        animator.SetBool("HasMoveInput", IsPressingMoveKey);
        animator.SetBool("HasJumpInput", playerInput.JumpInput);

        DealWithStemina();

        //待机
        TimeoutToIdle();

        CheckAiming();
        DealWithFlySword();
    }

    //初始化component
    private void InitialzePlayerComponent()
    {
        character = GetComponentInChildren<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();
    }

    //初始化动画
    private void InitializeAnimator()
    {
        animator.applyRootMotion = false;

        animatorCache = new AnimatorInfo(animator);

        animator.SetBool("IsGrounded", IsGrounded());
    }
    
    //初始化可受伤脚本
    private void InitilizeDamageable()
    {
        damageable.onDamageMessageReceivers.Add(this);
        damageable.isInvulnerable = true;
    }

    //设置出生地
    public void RegisterRespawnPos(Vector3 position)
    {
        Debug.Log("RegisterRespawnPoint");
        toRespawnAt = position;
    }

    //重置体力
    private void ResetStemina()
    {
        currentStemina = maxStemina;
    }

    //获得当前体力条比例
    public float GetCurrentSteminaRate()
    {
        return currentStemina / maxStemina;
    }

    //强制获得是否在地面信息
    private void ForceToGetIsGrounded(Vector3 moveDirection)
    {
        character.Move(moveDirection);
        //注意使用这种方法判断是否在地面必须要先Move()
        isGrounded = character.isGrounded;
    }

    //改变快速奔跑状态
    public void SetFastRunState(bool active)
    {
        isFastRunning = active;

        currentSpeed = active ? fastRunSpeed : moveSpeed;

        if (InWater()) currentSpeed = moveSpeedInWater;
    }

    //重置快速奔跑锁
    private IEnumerator ResetFastRunBlock()
    {
        yield return new WaitUntil(() => !playerInput.FastRunInput);
        fastRunBlock = false;
    }

    //计算体力
    private void DealWithStemina()
    {
        if (isFastRunning)
        {
            if (currentStemina > 0)
            {
                currentStemina = Mathf.Clamp(currentStemina - steminaCostForFasetRun, 0, maxStemina);
                isCostingStemina = true;
                isRecoveringStemina = false;
            }
            else
            {
                fastRunBlock = true;
                StartCoroutine(ResetFastRunBlock());
            }          
        }
        else
        {
            if (currentStemina < maxStemina)
            {
                currentStemina = Mathf.Clamp(currentStemina + steminaRecoverRate * steminaCostForFasetRun, 0, maxStemina);
                isCostingStemina = false;
                isRecoveringStemina = true;
            }
            else
            {
                isCostingStemina = false;
                isRecoveringStemina = false;
            }
        }
    }
    private void OnAnimatorMove()
    {
        if (isRespawning||waitToRespawn) return;

        Vector3 movement = Vector3.zero;
        if (IsGrounded()|| InWater())
        {

            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * 0.5f, -Vector3.up);
            if (Physics.Raycast(ray, out hit, 1f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                //用动画计算移动距离的一种方式
                movement = Vector3.ProjectOnPlane(animator.deltaPosition, hit.normal) * currentSpeed; 
            }
            else
            {
                movement = animator.deltaPosition * currentSpeed;
            }
        }
        else
        {
            Debug.Log("Move");
            movement = jumpForwardSpeed * curSpeedRef * transform.forward * Time.deltaTime;
        }

        character.transform.rotation *= animator.deltaRotation;

        //加入垂直速度
        movement += curVerticalSpeed * Vector3.up * Time.deltaTime;

        ForceToGetIsGrounded(movement);

        if (!IsGrounded()) animator.SetFloat("VerticalSpeed", curVerticalSpeed);

        animator.SetBool("IsGrounded", IsGrounded());
    }


    protected bool IsPressingMoveKey
    {
        get { return !Mathf.Approximately(playerInput.MoveInput.sqrMagnitude, 0f); }
    }

    void StartGliding()
    {
        playerInput.doubleBlock = true;
        currentGravity = glideGravity;
        isReadyToGlid = false;
        isGliding = true;
        glider.SetActive(true);

    }

    void EndGliding()
    {
        currentGravity = gravity;
        isReadyToGlid = true;
        isGliding = false;
        glider.SetActive(false);
    }

    void CalculateHorizontalMovement()
    {
        Vector2 moveInput = playerInput.MoveInput;
        if (moveInput.sqrMagnitude > 1)
            moveInput.Normalize();

        targetSpeedRef = moveInput.magnitude;

        float a = IsPressingMoveKey ? groundAcceleration : groundDeceleration;
        //Vt = Vo +at
        curSpeedRef = Mathf.MoveTowards(curSpeedRef, targetSpeedRef, Time.deltaTime * a);
    }

    void CalculateVerticalMovement()
    {
        if (!playerInput.JumpInput && IsGrounded())
        {
            isReadyToJump = true;
            EndGliding();
        }


        if (IsGrounded())
        {
            curVerticalSpeed = -10f;

            if (playerInput.JumpInput && isReadyToJump)
            {
                curVerticalSpeed = initialJumpSpeed;
                isGrounded = false;
                isReadyToJump = false;
            }
        }
        else
        {
            if (playerInput.JumpInput && isReadyToGlid && CanGlid())
            {
                Debug.Log("Glid");
                
                StartGliding();
            }
            else if (playerInput.JumpInput && isGliding && !isReadyToGlid)
            {
                EndGliding();
            }

            curVerticalSpeed -= 2 * currentGravity * Time.deltaTime;

        }
    }

    void CalculateRotation()
    {
        Vector3 moveInputDir = new Vector3(playerInput.MoveInput.x, 0, playerInput.MoveInput.y).normalized;
        Vector3 TPSCamForward = Quaternion.Euler(0, TPSCamera.m_XAxis.Value, 0) * Vector3.forward;

        Quaternion desiredRotation;
        if (Mathf.Approximately(Vector3.Dot(moveInputDir, Vector3.forward), -1)/*互为反向向量*/)
        {
            desiredRotation = Quaternion.LookRotation(-TPSCamForward);
        }
        else
        {
            Quaternion TPSCamForward2DesiredDir = Quaternion.FromToRotation(Vector3.forward, moveInputDir);
            desiredRotation = Quaternion.LookRotation(TPSCamForward2DesiredDir * TPSCamForward);
        }

        targetRotation = desiredRotation;

        Vector3 desiredDir = targetRotation * Vector3.forward;
        float currentAngle = Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(desiredDir.x, desiredDir.z) * Mathf.Rad2Deg;
        shortestDeltaRotDegree = Mathf.DeltaAngle(currentAngle, targetAngle);
    }

    void CharacterRotate()
    {
        animator.SetFloat("DeltaDeg2Rag", shortestDeltaRotDegree * Mathf.Deg2Rad);
        curRotateSpeed = maxRotateSpeed * curSpeedRef / maxSpeedRef;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, curRotateSpeed * Time.deltaTime);
    }

    void TimeoutToIdle()
    {
        bool inputDetected = IsPressingMoveKey || playerInput.JumpInput|| playerInput.AttackInput;

        if (IsGrounded() && !inputDetected)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTimeout)
            {
                idleTimer = 0f;
                animator.SetTrigger("IdleTrigger");
            }
        }
        else
        {
            idleTimer = 0f;
            animator.ResetTrigger("IdleTrigger");
        }
    }

    private bool IsGrounded()
    {
        if (curVerticalSpeed > 0) return false;
        float floorDistanceFromFoot = character.stepOffset;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, floorDistanceFromFoot, stepLayer) || character.isGrounded)
        {
            return true;
        }

        return false;
    }

    private bool InWater()
    {
        float floorDistanceFromFoot = waterOffset;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, floorDistanceFromFoot, waterLayer))
        {
            GameManager.Instance.firstWater = true;
            return true;

        }

        return false;
    }
    private bool CanGlid()
    {
        //3C暂时关闭glid
        return false;
        //if (curVerticalSpeed > 0) return false;
        //float floorDistanceFromFoot = character.stepOffset;

        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, stepLayer) || character.isGrounded)
        //{
        //    return false;
        //}

        //return true;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    //重生
    public void Respawn()
    {
        if (!isRespawning)
        {
            isRespawning = true;
            animator.SetTrigger("ShouldRespawn");
            StartCoroutine(ResetRespawnState());
        }

    }

    //重生重置状态
    private IEnumerator ResetRespawnState()
    {
        damageable.ResetDamage();
        yield return new WaitUntil(()=> animatorCache.currentStateInfo.IsName("Respawn") &&animatorCache.currentStateInfo.normalizedTime>1f);

        isRespawning = false;
    }

    void ShowHideMeleeWeapon(bool isShow)
    {
        if (meleeWeapons == null) return;
        foreach(var weapon in meleeWeapons)
        {
            weapon.gameObject.SetActive(isShow);
        }

        if (!isShow)
            animator.ResetTrigger("MeleeAttackTrigger");
    }

    bool IsWeaponAnimationOnPlay()
    {
        bool isWeaponEquipped;

        isWeaponEquipped = animatorCache.nextStateInfo.tagHash == Animator.StringToHash("WeaponEquippedAnim") || animatorCache.currentStateInfo.tagHash == Animator.StringToHash("WeaponEquippedAnim");

        return isWeaponEquipped;
    }

    //combo动画输入检测
    void DealWithMeleeAttackAnimation()
    {
        //动画归一化时间（0-1），在（0-1）上的repeat
        animator.SetFloat("MeleeStateTime", Mathf.Repeat(animatorCache.currentStateInfo.normalizedTime, 1f));
        
        //每一帧都reset，这样可以保证每次点击都触发一次trigger
        animator.ResetTrigger("MeleeAttackTrigger");

        if (playerInput.AttackInput)
        {
            animator.SetTrigger("MeleeAttackTrigger");
        }
    }

    void DealWithAimAttackAnimation()
    {
   animator.SetBool("IsAiming", isAiming);
    }
    public void MeleeAttackStart()
    {
        foreach(var weapon in meleeWeapons)
        {
            weapon.BeginAttack();
        }

        isAttacking = true;
    }

    // This is called by an animation event when Ellen finishes swinging her staff.
    public void MeleeAttackEnd()
    {
        foreach (var weapon in meleeWeapons)
        {
            weapon.EndAttack();
        }
        isAttacking = false;
    }

    public void OnReceiveMessage(MessageType type, object sender, object data)
    {

    }

    //更新可输入状态
    void UpdateInputBlock()
    {
        bool currentInputBlock = !animatorCache.isAnimatorTransitioning && animatorCache.currentStateInfo.tagHash == Animator.StringToHash("BlockInput");

        currentInputBlock |= animatorCache.nextStateInfo.tagHash == Animator.StringToHash("BlockInput");

        playerInput.inputBlock = currentInputBlock || inNarrative;
    }

    //缓存动画帧信息
    void CacheAnimatorState()
    {
        //上一帧动画信息记录
        animatorCache.previousCurrentStateInfo = animatorCache.currentStateInfo;
        animatorCache.previousIsAnimatorTransitioning = animatorCache.isAnimatorTransitioning;
        animatorCache.previousNextStateInfo = animatorCache.nextStateInfo;

        //当前帧动画信息更新
        animatorCache.currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animatorCache.isAnimatorTransitioning = animator.IsInTransition(0);
        animatorCache.nextStateInfo = animator.GetNextAnimatorStateInfo(0);
    }

    void CheckAiming()
    {
        if (playerInput.Aim != isAiming)
        {
            SetAim(playerInput.Aim);
            DealWithAimAttackAnimation();
        }
    }

    void DealWithFlySword()
    {
        if (playerInput.AttackInput)
        {
            if (playerInput.Aim &&!animatorCache.currentStateInfo.IsName("SilverSword")) {
                SilverSword.TriggerAbility();
            }
            
        }
    }

    void SetAim(bool aim)
    {
        isAiming = aim;

        if (aim)
        {
            transform.rotation = Quaternion.Euler(0f, TPSCamera.m_XAxis.Value, 0f);
            aimCamera.m_Priority = 50;
            //DOVirtual.Float(aimRig.weight, 1f, 0.2f, SetAimRigWeight);
        }
        else
        {
            aimCamera.m_Priority = 1;
            //DOVirtual.Float(aimRig.weight, 0, .2f, SetAimRigWeight);
        }
        //void SetAimRigWeight(float weight)
        //{
        //    aimRig.weight = weight;
        //}
    }

    void UpdateAimRotation()
    {
        var rot = headTarget.localRotation.eulerAngles;
        rot.x -= playerInput.CameraInput.y;
        if (rot.x > 180)
            rot.x -= 360;
        rot.x = Mathf.Clamp(rot.x, -80, 80);
        headTarget.localRotation = Quaternion.Slerp(headTarget.localRotation, Quaternion.Euler(rot), .5f);

        rot = transform.eulerAngles;
        rot.y += playerInput.CameraInput.x;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rot), .5f);
    }

    public void ApplySelfDamage(float damage, bool byFrame)
    {
        Damageable.DamageMessage data = new Damageable.DamageMessage();
        data.damage = byFrame? damage * Time.deltaTime :damage;
        data.damager = this;
        data.damageSource = transform.position;
        data.stopCamera = false;

        damageable.ApplyDamage(data);
    }

   IEnumerator RespawnToSavePoint()
    {
        waitToRespawn = true;
        SetPosition(toRespawnAt);
        animator.Rebind();
        animator.Play("Idle",-1,0);
        yield return new WaitForSeconds(0.1f);
        Respawn();

        waitToRespawn = false;
    }

    public float GetCurrentHp()
    {
        return damageable.currentHitPoints/ damageable.maxHitPoints;
    }

    public void Die()
    {
        StartCoroutine(RespawnToSavePoint());
    }
}
