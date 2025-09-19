using Cysharp.Threading.Tasks;
using HMFW;
using UnityEngine;

public partial class FsmDemo : MonoBehaviour
{
    private string haha = "hahah";
    public FSM<FsmDemo> Fsm;

    public async void Start()
    {
        Fsm = new FSM<FsmDemo>(this);
        Fsm.ChangeState(typeof(FsmStateDemo_Idle));
    }


    public void Update()
    {
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            dir += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            dir += Vector3.back;
        }

        if (Input.GetKey(KeyCode.A))
        {
            dir += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D))
        {
            dir += Vector3.right;
        }

        Fsm.GetCurrentStateInterface<IPlayerControlMove>()?.PlayerMove(dir);

        Fsm.FsmUpdate();
    }

    public void FixedUpdate()
    {
        Fsm.FsmFixedUpdate();
    }

    public void LateUpdate()
    {
        Fsm.FsmLateUpdate();
    }

    private void OnDestroy()
    {
        Fsm.FsmOnDestroy();
    }

    /// <summary>
    /// 生命
    /// </summary>
    private float currentHp = 10;


    [ContextMenu("BeHit")]
    public void BeHit()
    {
        float damage = 3;
        var realDamage = Fsm.GetCurrentStateInterface<IBeHit>()?.BeHit(damage);
        if (realDamage.HasValue)
        {
            currentHp -= realDamage.Value;
            Fsm.ChangeState<FsmStateDemo_BeHit>(); //进入受击状态
        }
    }

    [ContextMenu("BeResurgence")]
    public void BeResurgence()
    {
        Fsm.GetCurrentStateInterface<IResurgence>()?.Resurgence();
    }
}


public partial class FsmDemo
{
    /// <summary>
    /// 定義在宿主類之内的狀態類,可以直接訪問宿主類的所有內容包括private
    /// </summary>
    public class FsmStateDemo_Idle : FsmState<FsmDemo>, IPlayerControlMove, IBeHit
    {
        public override UniTask EnterState(params object[] args)
        {
            Debug.Log("進入 FsmStateDemo_Idle");

            Debug.Log(this.myFsm.Owner.haha); //可以訪問宿主的內部函數和變量

            return default;
        }

        public override void OnUpdate()
        {
        }

        public void PlayerMove(Vector3 dir)
        {
            // Debug.Log("FsmStateDemo_Idle PlayerMove");
            if (dir != Vector3.zero)
            {
                Debug.Log("FsmStateDemo_Idle PlayerMove  开始移动,转换到移动状态");
                myFsm.ChangeState<FsmStateDemoPlayerControlMove>();
            }
        }

        public float BeHit(float damage)
        {
            Debug.Log("FsmStateDemo_Idle damage");
            return 1;
        }
    }

    public class FsmStateDemoPlayerControlMove : FsmState<FsmDemo>, IPlayerControlMove, IBeHit
    {
        public override UniTask EnterState(params object[] args)
        {
            Debug.Log("進入 FsmStateDemo_Move");
            return default;
        }

        public void PlayerMove(Vector3 dir)
        {
            //Debug.Log("FsmStateDemo_Move PlayerMove");
            if (dir == Vector3.zero)
            {
                Debug.Log("FsmStateDemo_Move PlayerMove 移动速度为0,转换状态");
                myFsm.ChangeState<FsmStateDemo_Idle>();
            }
        }

        public float BeHit(float damage)
        {
            Debug.Log("FsmStateDemo_Move BeHid");
            return 1;
        }
    }


    public class FsmStateDemo_BeHit : FsmState<FsmDemo>, IBeHit
    {
        public override UniTask EnterState(params object[] args)
        {
            Debug.Log("進入 FsmStateDemo_BeHit");

            Debug.Log(this.myFsm.Owner.haha); //可以訪問宿主的內部函數和變量

            return default;
        }

        public override void OnUpdate()
        {
            if (myFsm.Owner.currentHp <= 0)
            {
                myFsm.ChangeState<FsmStateDemo_Dead>();
            }
        }

        public float BeHit(float damage)
        {
            Debug.Log("FsmStateDemo_BeHit damage");
            return 1;
        }
    }

    public class FsmStateDemo_Dead : FsmState<FsmDemo>, IResurgence
    {
        public override UniTask EnterState(params object[] args)
        {
            Debug.Log("進入 FsmStateDemo_Dead");
            return default;
        }

        public void Resurgence()
        {
            Debug.Log("我复活了");
            myFsm.ChangeState<FsmStateDemo_Idle>();
        }
    }
}

/// <summary>
/// 玩家控制移动接口,可以由玩家控制的状态实现这个接口
/// </summary>
public interface IPlayerControlMove : IFsmInterfaceBase
{
    void PlayerMove(Vector3 dir);
}

/// <summary>
/// 被击接口,可以被攻击的状态实现这个接口
/// </summary>
public interface IBeHit : IFsmInterfaceBase
{
    /// <summary>
    /// 传入伤害值,返回实际受到的伤害值(可能会有减伤等效果)
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    float BeHit(float damage);
}

/// <summary>
/// 复活接口,可以复活的状态实现这个接口
/// </summary>
public interface IResurgence : IFsmInterfaceBase
{
    void Resurgence();
}