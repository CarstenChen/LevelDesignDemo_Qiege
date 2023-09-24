using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    //定义单件
    public static PlayerInput pi_Instance;
    public static PlayerInput Instance
    {
        get
        {
            return pi_Instance;
        }

        private set { }
    }

    public bool inputBlock;
    public bool doubleBlock;
    public bool m_ExternalInputBlocked;

    protected Vector2 pl_MoveInput;
    protected bool pl_Jump;
    protected bool pl_Attack;
    protected bool pl_FastRun;
    protected bool pl_Interact;
    protected bool pl_Aim;
    protected Vector2 pl_Camera;


    public Vector2 MoveInput
    {
        get
        {
            if (inputBlock || m_ExternalInputBlocked) { return Vector2.zero; }
            return pl_MoveInput;
        }
    }
    public bool JumpInput { get { return pl_Jump && !inputBlock && !m_ExternalInputBlocked &&!doubleBlock; } }
    public bool AttackInput { get { return pl_Attack && !inputBlock && !m_ExternalInputBlocked; } }

    public bool FastRunInput { get { return pl_FastRun && !inputBlock && !m_ExternalInputBlocked; } }
    public bool Interact { get { return pl_Interact && !inputBlock && !m_ExternalInputBlocked; } }

    public bool Aim { get { return pl_Aim && !inputBlock && !m_ExternalInputBlocked; } }

    public Vector2 CameraInput
    {
        get
        {
            if (inputBlock || m_ExternalInputBlocked)
                return Vector2.zero;
            return pl_Camera;
        }
    }

    protected const float attackInputInterval = 0.03f;
    protected Coroutine currentCoroutine;

    void Awake()
    {
        //初始化单件
        if (pi_Instance == null)
            pi_Instance = this;
        else if (pi_Instance != this)
        {
            throw new UnityException("There can not be more than one PlayerInput Scripts");
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        pl_MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                pl_Jump = true;
                break;
            case InputActionPhase.Canceled:
                pl_Jump = false;
                doubleBlock = false;
                break;
            default:
                pl_Jump = false;
                break;
        }

    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                pl_Interact = true;
                break;
            default:
                pl_Interact = false;
                break;
        }

    }

    public void OnFastRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                pl_FastRun = true;
                break;
            case InputActionPhase.Performed:
                pl_FastRun = true;
                break;
            case InputActionPhase.Canceled:
                pl_FastRun = false;
                break;

        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        //冲掉前一个输入，保持attack是true
        if(currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(SetAttackParameter());
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                pl_Aim = true;
                break;
            case InputActionPhase.Performed:
                pl_Aim = true;
                break;
            case InputActionPhase.Canceled:
                pl_Aim = false;
                break;

        }
    }

    IEnumerator SetAttackParameter()
    {
        pl_Attack = true;
        yield return new WaitForSeconds(attackInputInterval);
        pl_Attack = false;
    }

    public bool HaveControl()
    {
        return !m_ExternalInputBlocked;
    }

    public void ReleaseControl()
    {
        m_ExternalInputBlocked = true;
    }

    public void GainControl()
    {
        m_ExternalInputBlocked = false;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        pl_Camera = context.ReadValue<Vector2>();
    }
}
