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
    }
}