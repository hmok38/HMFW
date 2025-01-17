using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace HMFW
{
    /// <summary>
    /// 时间管理器,可以标识时间是否经过校正,防止修改本地时间
    /// </summary>
    public class TimeSyncMgr : TimeSyncMgrBase
    {
        /// <summary>
        /// 同步时候的unity时间
        /// </summary>
        private double _syncUnityUnscaleTime;

        public override Action timeBeSyncedAction { get; set; }

        /// <summary>
        /// 同步时候获得的服务器时间
        /// </summary>
        private DateTime _syncServerDateTime;

        /// <summary>
        /// 是否已经同步过时间了
        /// </summary>
        public override bool hadSyncTime { get; protected set; }


        private readonly string _utcUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";
        private Func<UniTask<TimeSyncResult>> _timeSyncFunc;
        private bool _beSyncing;
        int _retryAttempt;
        private bool _beCalledSync;


        /// <summary>
        /// 设置自定义时间同步函数
        /// </summary>
        /// <param name="timeSyncFunc"></param>
        public override void SetTimeSyncFunc(Func<UniTask<TimeSyncResult>> timeSyncFunc)
        {
            _timeSyncFunc = timeSyncFunc;
        }

        /// <summary>
        /// 发起时间同步,会优先使用SetTimeSyncFunc设置的自定义的func,
        /// 如果没有自定义,则用框架自带的从worldtimeapi.org获取时间
        /// </summary>
        public override void SyncTime()
        {
            if (_beCalledSync) return; //保证只调用一次
            _beCalledSync = true;
            SyncTimeHandle().Forget();
        }

        private async UniTask SyncTimeHandle()
        {
            if (_beSyncing) return; //正在同步,不会重复调用
            TimeSyncResult result = null;
            if (_timeSyncFunc != null)
            {
                result = await SyncTimeHandle(_timeSyncFunc);
            }

            if (result == null || result.Result != UnityWebRequest.Result.Success) //没有成功
            {
                //调用base的同步函数
                result = await SyncTimeHandle(BaseTimeSyncFunc);
            }

            if (result.Result != UnityWebRequest.Result.Success)
            {
                //还是没成功,则重试
                DelayLoad().Forget();
            }
            else
            {
                timeBeSyncedAction?.Invoke();
                _retryAttempt = 0;
            }
        }

        private async UniTask DelayLoad()
        {
            _retryAttempt++;
            double retryDelay = Math.Pow(5, Math.Min(3, _retryAttempt)); //5秒,25秒/,125秒
            await UniTask.Delay((int)(retryDelay * 1000));
            SyncTimeHandle().Forget();
        }

        /// <summary>
        /// 获取服务器时间,如果没有经过同步,则返回来自系统的时间(已经经过utc时间校正转成了本地时间)
        /// </summary>
        /// <returns></returns>
        public override DateTime GetServerDateTime()
        {
            if (hadSyncTime)
            {
                return _syncServerDateTime.AddMilliseconds((Time.unscaledTimeAsDouble - _syncUnityUnscaleTime) * 1000);
            }
            else
            {
                return DateTime.Now;
            }
        }

        private async UniTask<TimeSyncResult> SyncTimeHandle(Func<UniTask<TimeSyncResult>> func)
        {
            try
            {
                Debug.Log("正在请求同步时间");
                _beSyncing = true;
                var time = Time.unscaledTimeAsDouble;
                var result = await func.Invoke();

                _beSyncing = false;
                if (result.Result == UnityWebRequest.Result.Success)
                {
                    _syncUnityUnscaleTime = time + (Time.unscaledTimeAsDouble - time) / 2f; //扣除网络延时
                    _syncServerDateTime = result.Time;
                    hadSyncTime = true;
                    Debug.Log(
                        $"同步时间成功,同步函数名:{func.Method.Name}  time:{result.Time:yyyyMMdd HH:mm:ss fff} ");
                    return result;
                }
                else
                {
                    Debug.LogError(
                        $"同步时间错误,同步函数名:{func.Method.Name} Result:{result.Result} error:{result.ErrorMsg} code:{result.ResponseCode}");

                    return result;
                }
            }
            catch (Exception e)
            {
                _beSyncing = false;
                Debug.LogError($"同步时间时发生错误:同步函数名:{func.Method.Name} 错误:{e}");
                //Debug.LogException(e);
                return null;
            }
        }

        private async UniTask<TimeSyncResult> BaseTimeSyncFunc()
        {
            try
            {
                UnityWebRequest request = UnityWebRequest.Get(_utcUrl);
                request.timeout = 5;
                var op = request.SendWebRequest();
                while (!op.isDone)
                {
                    await UniTask.Delay(100);
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // 解析时间
                    string json = request.downloadHandler.text;
                    Debug.Log($"BaseTimeSyncFunc downloadHandler:  {request.downloadHandler.text}");
                    var serverTime = ParseServerTime(json);
                    Debug.Log($"同步时间成功! time: {serverTime:yyyy MMMM dd HH:mm:ss fff}");
                    Debug.Log($"同步时间成功! time: {serverTime:yyyy MMMM dd HH:mm:ss fff zzz} Utc:{serverTime.ToUniversalTime():yyyy MMMM dd HH:mm:ss fff zzz}");
                    return new TimeSyncResult()
                    {
                        Result = request.result,
                        Time = serverTime,
                        ErrorMsg = request.error,
                        ResponseCode = request.responseCode
                    };
                }
                else
                {
                    Debug.LogError($"同步时间失败: {request.error}");
                    return new TimeSyncResult()
                    {
                        Result = request.result,
                        ErrorMsg = request.error,
                        ResponseCode = request.responseCode
                    };
                }
            }
            catch (Exception e)
            {
                _beSyncing = false;
                Debug.LogError($"同步时间时发生错误:同步函数名:BaseTimeSyncFunc 错误:{e}");
                //Debug.LogException(e);
                return null;
            }
        }

        private DateTime ParseServerTime(string json)
        {
            // 简单 JSON 解析（适用于 WorldTimeAPI 的响应格式）
            int dateTimeIndex = json.IndexOf("\"dateTime\":\"") + 12;
            string dateTimeString = json.Substring(dateTimeIndex, 27) + "Z"; // 获取 ISO 8601 格式时间 "z"代表的是utc时间
            Debug.Log($"dateTimeString {dateTimeString}");

            return DateTime.Parse(dateTimeString);
        }
    }

    public abstract class TimeSyncMgrBase
    {
        /// <summary>
        /// 可以添加时间被注册后的触发事件
        /// </summary>
        public abstract Action timeBeSyncedAction { get; set; }

        /// <summary>
        /// 是否已经同步过时间了
        /// </summary>
        public abstract bool hadSyncTime { get; protected set; }

        /// <summary>
        /// 设置自定义时间同步函数
        /// </summary>
        /// <param name="timeSyncFunc">自定义的时间同步函数,记得返回的DataTime要转回本地时间</param>
        public abstract void SetTimeSyncFunc(Func<UniTask<TimeSyncResult>> timeSyncFunc);

        /// <summary>
        /// 发起时间同步,会优先使用SetTimeSyncFunc设置的自定义的func,
        /// 如果没有自定义,则用框架自带的从worldtimeapi.org获取时间
        /// </summary>
        public abstract void SyncTime();

        /// <summary>
        /// 获取服务器时间,如果没有经过同步,则返回来自系统的时间(已经经过utc时间校正转成了本地时间)
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GetServerDateTime();
    }

    public class TimeSyncResult
    {
        public DateTime Time;
        public string ErrorMsg;
        public long ResponseCode;
        public UnityWebRequest.Result Result;
    }
}