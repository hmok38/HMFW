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
        await UniTask.Delay(1000);
        Fsm.ChangeState(typeof(FsmStateDemo));
        await UniTask.Delay(1000);
        Fsm.ChangeState<FsmStateDemo2>();
        await UniTask.Delay(1000);
        Fsm.ChangeState("FsmStateDemo");
    }


    public void Update()
    {
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
}


public partial class FsmDemo
{
    /// <summary>
    /// 定義在宿主類之内的狀態類,可以直接訪問宿主類的所有內容包括private
    /// </summary>
    public class FsmStateDemo : FsmState<FsmDemo>
    {
        public override UniTask EnterState(params object[] args)
        {
            Debug.Log("進入FsmStateDemo");

            Debug.Log(this.myFsm.Owner.haha); //可以訪問宿主的內部函數和變量

            return default;
        }

        public override void OnUpdate()
        {
        }
    }

    public class FsmStateDemo2 : FsmState<FsmDemo>
    {
        public override UniTask EnterState(params object[] args)
        {
            Debug.Log("進入FsmStateDemo2");
            return default;
        }
    }
}