using UnityEngine;
using UnityEngine.Events;

namespace HMFW
{
    public class VibrationMgr : VibrationMgrBase
    {
        private const string VibrationPrefsKey = "VibrationPrefsKey";

        /// <summary>
        /// 开关状态
        /// </summary>
        public override bool beOn
        {
            get => _beOn;
            set
            {
                var beChange = value != _beOn;
                _beOn = value;
                PlayerPrefs.SetString(VibrationPrefsKey, _beOn.ToString());
                if (beChange)
                {
                    _onSwitchChange?.Invoke(_beOn);
                }
            }
        }

        private bool _beOn;
        private UnityAction<bool> _onSwitchChange;

        public VibrationMgr()
        {
            _beOn = true;
            if (PlayerPrefs.HasKey(VibrationPrefsKey))
            {
                if (bool.TryParse(PlayerPrefs.GetString(VibrationPrefsKey), out bool beOnResult))
                {
                    this._beOn = beOnResult;
                }
            }
        }


        /// <summary>
        /// 监听开关切换调整事件
        /// </summary>
        /// <param name="onSwitchChange"></param>
        public override void AddSwitchChange(UnityAction<bool> onSwitchChange)
        {
            _onSwitchChange += onSwitchChange;
        }

        /// <summary>
        /// 取消监听开关调整事件
        /// </summary>
        /// <param name="onSwitchChange"></param>
        public override void RemoveSwitchChange(UnityAction<bool> onSwitchChange)
        {
            _onSwitchChange -= onSwitchChange;
        }

        /// <summary>
        /// 触发简单震动
        /// </summary>
        public override void TriggerSampleVibration()
        {
            if (beOn)
                Handheld.Vibrate();
        }
        
        /// <summary>
        /// 触发指定长度的震动(安卓)
        /// ios下小于0.500则为短震动 等于0.500则为unity标准震动 大于0.5则为ios原生长震动
        /// </summary>
        /// <param name="milliseconds"></param>
        public override  void TriggerVibration(int milliseconds)
        {
            if (!beOn) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject vibrator = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")
                .Call<AndroidJavaObject>("getSystemService", "vibrator");
            vibrator.Call("vibrate", (long)(milliseconds));
#elif UNITY_IOS && !UNITY_EDITOR
       TriggerSampleVibration();
            // if (milliseconds == 500)
            // {
            //     TriggerSampleVibration();
            // }
            // else
            // {
            //     int id = milliseconds < 500 ? 1519 : 1520;
            //     AudioServicesPlaySystemSound(id);
            //
            //     // AudioServicesPlaySystemSound(1519); // 短震动ID
            //     // AudioServicesPlaySystemSound(1520); // 长震动ID
            // }
#else
            TriggerSampleVibration();
#endif
        }
    }

    public abstract class VibrationMgrBase
    {
        /// <summary>
        /// 开关状态
        /// </summary>
        public abstract bool beOn { get; set; }

        /// <summary>
        /// 触发简单震动
        /// </summary>
        public abstract void TriggerSampleVibration();

        /// <summary>
        /// 监听开关切换调整事件
        /// </summary>
        /// <param name="onSwitchChange"></param>
        public abstract void AddSwitchChange(UnityAction<bool> onSwitchChange);

        /// <summary>
        /// 取消监听开关调整事件
        /// </summary>
        /// <param name="onSwitchChange"></param>
        public abstract void RemoveSwitchChange(UnityAction<bool> onSwitchChange);

        /// <summary>
        /// 触发指定长度的震动(安卓)
        /// ios下小于0.500则为短震动 等于0.500则为unity标准震动 大于0.5则为ios原生长震动
        /// </summary>
        /// <param name="milliseconds"></param>
        public abstract void TriggerVibration(int milliseconds);
    }
}