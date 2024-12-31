using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class AudioMgr : AudioMgrBase
{
    private const string SoundPrefsKey = "SoundPrefsKey";
    private const string MusicPrefsKey = "MusicPrefsKey";
    private float _soundVolume = 1;
    private float _musicVolume = 1;
    private float _musicPitch = 1;
    protected readonly Dictionary<Enum, AudioInfo> AudioClips = new Dictionary<Enum, AudioInfo>();

    private readonly AudioSource _musicAudioSource;
    private UnityAction<Enum, string> _musicCompleteCb;
    private Enum _lastMusicEnum;
    private string _lastMusicFlagStr;

    private bool _musicBeLoop;

    CancellationTokenSource _musicCompleteCbCancelTokenS = new CancellationTokenSource();

    //管理不可叠加的音效(只存在唯一音频)
    private readonly Dictionary<Enum, AudioSource> _audioSources = new Dictionary<Enum, AudioSource>();

    //不可叠加音效的管理节点
    private GameObject _audioLayer;

    //可叠加音效的管理节点
    private GameObject _audioPoolLayer;

    /// <summary>
    /// 不可复用的音效节点
    /// </summary>
    private GameObject audioLayer
    {
        get
        {
            if (_audioLayer == null)
                _audioLayer = new GameObject("AudioLayer");
            return _audioLayer;
        }
    }

    /// <summary>
    /// 可复用的音效节点
    /// </summary>
    private GameObject audioPoolLayer
    {
        get
        {
            if (_audioPoolLayer == null)
                _audioPoolLayer = new GameObject("AudioPoolLayer");
            return _audioPoolLayer;
        }
    }

    /// <summary>
    /// 背景音乐是否在播放
    /// </summary>
    public override bool beMusicPlaying => _musicAudioSource != null && _musicAudioSource.isPlaying;

    /// <summary>
    /// 正在或者上一个播放的音乐
    /// </summary>
    public override Enum lastMusic => _lastMusicEnum;

    /// <summary>
    /// 正在或者上一个播放的音乐是否是循环的
    /// </summary>
    public override bool lastMusicLoop => _musicBeLoop;

    /// <summary>
    /// 注册音量变化的事件
    /// </summary>
    private UnityAction _onVolumeChange;

    /// <summary>
    /// 监听音量调整事件
    /// </summary>
    /// <param name="onVolumeChange"></param>
    public override void AddVolumeChange(UnityAction onVolumeChange)
    {
        _onVolumeChange += onVolumeChange;
    }

    /// <summary>
    /// 取消监听音量调整事件
    /// </summary>
    /// <param name="onVolumeChange"></param>
    public override void RemoveVolumeChange(UnityAction onVolumeChange)
    {
        _onVolumeChange -= onVolumeChange;
    }

    public override float soundVolume
    {
        get => _soundVolume;
        set
        {
            var v = Mathf.Clamp(value, 0f, 1f);
            PlayerPrefs.SetFloat(SoundPrefsKey, v);
            _soundVolume = v;
            RefreshSoundVolume(audioLayer);
            RefreshSoundVolume(audioPoolLayer);
            _onVolumeChange?.Invoke();
        }
    }

    public override float musicVolume
    {
        get => _musicVolume;
        set
        {
            var v = Mathf.Clamp(value, 0f, 1f);
            PlayerPrefs.SetFloat(MusicPrefsKey, v);
            _musicVolume = v;
            _musicAudioSource.volume = v;
            _onVolumeChange?.Invoke();
        }
    }

    public override float musicPitch
    {
        get => _musicPitch;
        set
        {
            _musicPitch = value;
            if (_musicAudioSource != null)
            {
                _musicAudioSource.pitch = _musicPitch;
            }
        }
    }

    public AudioMgr()
    {
        if (PlayerPrefs.HasKey(SoundPrefsKey))
        {
            _soundVolume = PlayerPrefs.GetFloat(SoundPrefsKey);
        }

        if (PlayerPrefs.HasKey(MusicPrefsKey))
        {
            _musicVolume = PlayerPrefs.GetFloat(MusicPrefsKey);
        }

        var obj = new GameObject("MusicAudioSource");
        UnityEngine.Object.DontDestroyOnLoad(obj);
        _musicAudioSource = obj.AddComponent<AudioSource>();
        _musicAudioSource.volume = _musicVolume;
    }

    /// <summary>
    /// 增加音频
    /// </summary>
    /// <param name="enum"></param>
    /// <param name="audioClip"></param>
    /// <param name="forceReplay">这个音频如果正在播放时,如果再次播放,则强制从头开始播放(canMultiplePlay=false时生效)</param>
    /// <param name="canMultiplePlay">这个音频是否可同时播放多个,不允许时,</param>
    public override void AddAudioClip(Enum @enum, AudioClip audioClip, bool canMultiplePlay = true,
        bool forceReplay = false)
    {
        AudioClips[@enum] = new AudioInfo()
            { Clip = audioClip, CanMultiplePlay = canMultiplePlay, ForceReplay = forceReplay };
    }

    /// <summary>
    /// 移除音频
    /// </summary>
    /// <param name="enum"></param>
    public override void RemoveAudioClip(Enum @enum)
    {
        AudioClips.Remove(@enum);
    }

    public override AudioClip GetAudioClip(Enum @enum)
    {
        AudioClips.TryGetValue(@enum, out var audioInfo);
        return audioInfo?.Clip;
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="enum">音乐的枚举</param>
    /// <param name="completeFlag">完成时回调的标签,不需要可以为空,会原样传回</param>
    /// <param name="completeCb">完成时的回调,不需要可以为空</param>
    /// <param name="beLoop">是否需要loop,默认需要</param>
    public override async void PlayMusic(Enum @enum, bool beLoop = true, string completeFlag = "",
        UnityAction<Enum, string> completeCb = null
    )
    {
        if (AudioClips.TryGetValue(@enum, out var audioInfo))
        {
            try
            {
                if (_musicCompleteCbCancelTokenS != null)
                {
                    _musicCompleteCbCancelTokenS.Cancel(); //取消之前等待的执行逻辑
                    _musicCompleteCbCancelTokenS.Dispose();
                }

                _musicCompleteCbCancelTokenS = new CancellationTokenSource();

                if (_lastMusicEnum == null || !_lastMusicEnum.Equals(@enum)) //不同的就恢复正常音调
                {
                    musicPitch = 1;
                }

                _musicAudioSource.clip = audioInfo.Clip;
                _musicAudioSource.Play();
                _musicCompleteCb = completeCb;
                _lastMusicEnum = @enum;
                _lastMusicFlagStr = completeFlag;
                _musicBeLoop = beLoop;
                _musicAudioSource.loop = false; //不自动loop
                await UniTask.WaitUntil(() => _musicAudioSource != null && !_musicAudioSource.isPlaying,
                    cancellationToken: _musicCompleteCbCancelTokenS.Token);

                if (_musicBeLoop)
                {
                    PlayMusic(_lastMusicEnum, beLoop, _lastMusicFlagStr, completeCb);
                }

                _musicCompleteCb?.Invoke(_lastMusicEnum, _lastMusicFlagStr);
            }
            catch
            {
                //隐藏取消抛出的错误
            }
        }
        else
        {
            Debug.LogError($"PlayMusic not exist music : {@enum}");
        }
    }

    /// <summary>
    /// 更新音效声音
    /// </summary>
    /// <param name="parentObject"></param>
    private void RefreshSoundVolume(GameObject parentObject)
    {
        AudioSource[] audioSources = parentObject.GetComponents<AudioSource>();
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].volume = _soundVolume;
        }
    }

    /// <summary>
    /// 可叠加播放的类型不做播放器管理
    /// 只能存在一个的才做播放器管理
    /// </summary>
    /// <param name="enum"></param>
    public override void PlaySound(Enum @enum)
    {
        if (AudioClips.TryGetValue(@enum, out var audioInfo))
        {
            if (audioInfo.CanMultiplePlay)
            {
                //遍历找空闲的audioSource
                AudioSource targetAudioSource = null;
                AudioSource[] audioSources = audioPoolLayer.GetComponents<AudioSource>();
                for (int i = 0; i < audioSources.Length; i++)
                {
                    if (!audioSources[i].isPlaying)
                    {
                        targetAudioSource = audioSources[i];
                        break;
                    }
                }
                //如果遍历完还没找到空闲的audioSource就创建一个新的audioSource使用
                if (targetAudioSource == null)
                {
                    if (audioSources.Length >= 20)
                        Debug.LogError($"当前存在的音频数量为:{audioSources.Length}");
                    targetAudioSource = (AudioSource)audioPoolLayer.AddComponent(typeof(AudioSource));
                }
                targetAudioSource.clip = audioInfo.Clip;
                targetAudioSource.spatialBlend = 0f;
                targetAudioSource.volume = _soundVolume;
                targetAudioSource.Play();
            }
            else
            {
                //获取当前只能存在唯一的音效看有没有对应的播放器
                if (!_audioSources.TryGetValue(@enum, out AudioSource audioSource) || audioSource == null)
                {
                    //创建播放器
                    GameObject gameObject = new GameObject(@enum.ToString());
                    gameObject.transform.parent = audioLayer.transform;
                    audioSource = gameObject.AddComponent<AudioSource>();
                    _audioSources[@enum] = audioSource;
                }

                audioSource.clip = audioInfo.Clip;

                if (audioInfo.ForceReplay || !audioSource.isPlaying)
                {
                    audioSource.time = 0;
                    audioSource.Play();
                }
            }
        }
        else
        {
            Debug.LogError($"PlaySound not exist Sound : {@enum}");
        }
    }


    /// <summary>
    /// 停止音乐
    /// </summary>
    public override void StopMusic()
    {
        try
        {
            _musicCompleteCbCancelTokenS.Cancel(); //取消之前等待的执行逻辑
            _musicCompleteCbCancelTokenS.Dispose();
            _musicCompleteCbCancelTokenS = null;
            _musicAudioSource.Stop();
        }
        catch
        {
            // 隐藏取消抛出的错误
        }
    }

    /// <summary>
    /// 停止音效
    /// </summary>
    public override void StopSound()
    {
        StopSound(audioLayer);
        StopSound(audioPoolLayer);
    }

    private void StopSound(GameObject parentObject)
    {
        AudioSource[] audioSources = parentObject.GetComponents<AudioSource>();
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].volume = _soundVolume;
        }
    }


    ~AudioMgr()
    {
        if (_musicCompleteCbCancelTokenS != null)
        {
            try
            {
                _musicCompleteCbCancelTokenS.Cancel(); //取消之前等待的执行逻辑
                _musicCompleteCbCancelTokenS.Dispose();
            }
            catch
            {
                //隐藏取消抛出的错误
            }
        }
    }
}

public abstract class AudioMgrBase
{
    /// <summary>
    /// 声音音量
    /// </summary>
    public abstract float soundVolume { get; set; }

    /// <summary>
    /// 音乐音量
    /// </summary>
    public abstract float musicVolume { get; set; }


    /// <summary>
    /// 音乐音调和播放速度
    /// </summary>
    public abstract float musicPitch { get; set; }

    /// <summary>
    /// 播放音乐,仅一个音源,播放另外一个时,会停止之前的
    /// </summary>
    /// <param name="enum">音乐的枚举</param>
    /// <param name="completeFlag">完成时回调的标签,不需要可以为空,会原样传回</param>
    /// <param name="completeCb">完成时的回调,不需要可以为空</param>
    /// <param name="beLoop">是否需要loop,默认需要</param>
    public abstract void PlayMusic(Enum @enum, bool beLoop = true, string completeFlag = "",
        UnityAction<Enum, string> completeCb = null
    );


    /// <summary>
    /// 播放音效,会根据添加时的
    /// </summary>
    /// <param name="enum"></param>
    public abstract void PlaySound(Enum @enum);

    /// <summary>
    /// 增加音频
    /// </summary>
    /// <param name="enum"></param>
    /// <param name="audioClip"></param>
    /// <param name="forceReplay">这个音频如果正在播放时,如果再次播放,则强制从头开始播放(canMultiplePlay=false时生效)</param>
    /// <param name="canMultiplePlay">这个音频是否可同时播放多个,不允许时,</param>
    public abstract void AddAudioClip(Enum @enum, AudioClip audioClip, bool canMultiplePlay = true,
        bool forceReplay = false);

    /// <summary>
    /// 移除音频
    /// </summary>
    /// <param name="enum"></param>
    public abstract void RemoveAudioClip(Enum @enum);

    /// <summary>
    /// 获取clip
    /// </summary>
    /// <param name="enum"></param>
    /// <returns></returns>
    public abstract AudioClip GetAudioClip(Enum @enum);

    /// <summary>
    /// 背景音乐是否在播放
    /// </summary>
    public abstract bool beMusicPlaying { get; }

    /// <summary>
    /// 正在或者上一个播放的音乐
    /// </summary>
    public abstract Enum lastMusic { get; }

    /// <summary>
    /// 正在或者上一个播放的音乐是否是循环的
    /// </summary>
    public abstract bool lastMusicLoop { get; }


    /// <summary>
    /// 停止音乐
    /// </summary>
    public abstract void StopMusic();

    /// <summary>
    /// 停止音效
    /// </summary>
    public abstract void StopSound();

    /// <summary>
    /// 监听音量调整事件
    /// </summary>
    /// <param name="onVolumeChange"></param>
    public abstract void AddVolumeChange(UnityAction onVolumeChange);

    /// <summary>
    /// 取消音量调整事件
    /// </summary>
    /// <param name="onVolumeChange"></param>
    public abstract void RemoveVolumeChange(UnityAction onVolumeChange);

    protected class AudioInfo
    {
        public AudioClip Clip;
        public bool ForceReplay;
        public bool CanMultiplePlay;
    }
}