using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class AudioMgr : AudioMgrBase
{
    private const string SoundPrefsKey = "SoundPrefsKey";
    private const string MusicPrefsKey = "MusicPrefsKey";
    private float _soundVolume = 1;
    private float _musicVolume = 1;
    private float _musicPitch = 1;
    protected readonly Dictionary<string, AudioInfo> AudioClips = new Dictionary<string, AudioInfo>();

    private readonly AudioSource _musicAudioSource;
    private string _lastMusicEnum;
    private string _lastMusicFlagStr;

    private bool _musicBeLoop;
    private bool _bePause = false;
    private CancellationTokenSource _musicCompleteCbCancelTokenS;

    //管理不可叠加的音效(只存在唯一音频)
    private readonly Dictionary<string, AudioSource> _audioSources = new Dictionary<string, AudioSource>();
    private readonly Dictionary<AudioSource, string> _audioPoolSources = new Dictionary<AudioSource, string>();

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
            {
                _audioLayer = new GameObject("AudioLayer");
                UnityEngine.Object.DontDestroyOnLoad(_audioLayer);
            }

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
            {
                _audioPoolSources.Clear();
                _audioPoolLayer = new GameObject("AudioPoolLayer");
                UnityEngine.Object.DontDestroyOnLoad(_audioPoolLayer);
            }

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
    public override string lastMusic => _lastMusicEnum;

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
            if (_musicAudioSource != null)
            {
                _musicAudioSource.volume = v;
            }
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
    /// 增加音频，可叠加音效的最短播放间隔默认为 0.1 秒
    /// </summary>
    /// <param name="enum"></param>
    /// <param name="audioClip"></param>
    /// <param name="forceReplay">这个音频如果正在播放时,如果再次播放,则强制从头开始播放(canMultiplePlay=false时生效)</param>
    /// <param name="canMultiplePlay">这个音频是否可同时播放多个,不允许时,</param>
    /// <param name="multiplePlayInterval">可叠加音效的间隔秒数，0 表示不限制；音乐不受影响。</param>
    public override void AddAudioClip(Enum @enum, AudioClip audioClip, bool canMultiplePlay = true,
        bool forceReplay = false, float multiplePlayInterval = 0.1f)
    {
        AddAudioClip(@enum.ToString(), audioClip, canMultiplePlay, forceReplay, multiplePlayInterval);
    }

    /// <summary>
    /// 增加音频，可叠加音效的最短播放间隔默认为 0.1 秒
    /// </summary>
    /// <param name="enum"></param>
    /// <param name="audioClip"></param>
    /// <param name="forceReplay">这个音频如果正在播放时,如果再次播放,则强制从头开始播放(canMultiplePlay=false时生效)</param>
    /// <param name="canMultiplePlay">这个音频是否可同时播放多个,不允许时,</param>
    /// <param name="multiplePlayInterval">可叠加音效的间隔秒数，0 表示不限制；音乐不受影响。</param>
    public override void AddAudioClip(string @enum, AudioClip audioClip, bool canMultiplePlay = true,
        bool forceReplay = false, float multiplePlayInterval = 0.1f)
    {
        if (audioClip == null)
        {
            throw new ArgumentNullException(nameof(audioClip), "注册的音频不能为空或已被销毁。");
        }

        if (multiplePlayInterval < 0f || float.IsNaN(multiplePlayInterval) || float.IsInfinity(multiplePlayInterval))
        {
            throw new ArgumentOutOfRangeException(nameof(multiplePlayInterval), "音效播放间隔必须为有限的非负秒数。");
        }

        AudioClips[@enum.ToString()] = new AudioInfo()
        {
            Clip = audioClip,
            CanMultiplePlay = canMultiplePlay,
            ForceReplay = forceReplay,
            MultiplePlayInterval = multiplePlayInterval
        };
    }

    /// <summary>
    /// 移除音频，停止对应播放并清理音效播放器
    /// </summary>
    /// <param name="enum"></param>
    public override void RemoveAudioClip(Enum @enum)
    {
        RemoveAudioClip(@enum.ToString());
    }

    /// <summary>
    /// 移除音频，停止对应播放并清理音效播放器
    /// </summary>
    /// <param name="enum"></param>
    public override void RemoveAudioClip(string @enum)
    {
        AudioClips.Remove(@enum);

        if (_audioSources.TryGetValue(@enum, out var audioSource))
        {
            _audioSources.Remove(@enum);
            DestroySoundSource(audioSource);
        }

        foreach (var source in new List<AudioSource>(_audioPoolSources.Keys))
        {
            if (_audioPoolSources[source] == @enum)
            {
                _audioPoolSources.Remove(source);
                DestroySoundSource(source);
            }
        }

        if (_lastMusicEnum == @enum)
        {
            StopMusic();
            if (_musicAudioSource != null)
            {
                _musicAudioSource.clip = null;
            }
        }
    }

    private static void DestroySoundSource(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = null;
        UnityEngine.Object.Destroy(audioSource);
    }

    public override AudioClip GetAudioClip(Enum @enum)
    {
        return GetAudioClip(@enum.ToString());
    }

    public override AudioClip GetAudioClip(string @enum)
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
    public override void PlayMusic(Enum @enum, bool beLoop = true, string completeFlag = "",
        UnityAction<string, string> completeCb = null
    )
    {
        PlayMusic(@enum.ToString(), beLoop, completeFlag, completeCb);
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="enum">音乐的枚举</param>
    /// <param name="completeFlag">完成时回调的标签,不需要可以为空,会原样传回</param>
    /// <param name="completeCb">完成时的回调,不需要可以为空</param>
    /// <param name="beLoop">是否需要loop,默认需要</param>
    public override async void PlayMusic(string @enum, bool beLoop = true, string completeFlag = "",
        UnityAction<string, string> completeCb = null
    )
    {
        if (!AudioClips.TryGetValue(@enum, out var audioInfo))
        {
            Debug.LogError($"PlayMusic not exist music : {@enum}");
            return;
        }

        if (audioInfo.Clip == null || _musicAudioSource == null)
        {
            Debug.LogError($"PlayMusic 音频或音乐播放器已被销毁：{@enum}");
            return;
        }

        StopMusic();
        var cancelTokenSource = new CancellationTokenSource();
        _musicCompleteCbCancelTokenS = cancelTokenSource;
        try
        {
            if (_lastMusicEnum == null || !_lastMusicEnum.Equals(@enum)) //不同的就恢复正常音调
            {
                musicPitch = 1;
            }

            _musicAudioSource.clip = audioInfo.Clip;
            _musicAudioSource.Play();
            _lastMusicEnum = @enum;
            _lastMusicFlagStr = completeFlag;
            _musicBeLoop = beLoop;
            _musicAudioSource.loop = false; //不自动loop
            try
            {
                await UniTask.WaitUntil(
                    () => _musicAudioSource == null || audioInfo.Clip == null ||
                          (!_musicAudioSource.isPlaying && !_bePause),
                    cancellationToken: cancelTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (_musicAudioSource == null || audioInfo.Clip == null)
            {
                return;
            }

            if (_musicBeLoop)
            {
                PlayMusic(_lastMusicEnum, beLoop, _lastMusicFlagStr, completeCb);
            }

            completeCb?.Invoke(@enum, completeFlag);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            if (_musicCompleteCbCancelTokenS == cancelTokenSource)
            {
                _musicCompleteCbCancelTokenS = null;
                cancelTokenSource.Dispose();
                _bePause = false;
            }
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
    /// 可叠加音效复用空闲播放器，不可叠加音效使用独立播放器
    /// </summary>
    /// <param name="enum"></param>
    public override void PlaySound(Enum @enum)
    {
        PlaySound(@enum.ToString());
    }

    /// <summary>
    /// 可叠加音效复用空闲播放器，不可叠加音效使用独立播放器
    /// </summary>
    /// <param name="enum"></param>
    public override void PlaySound(string @enum)
    {
        if (AudioClips.TryGetValue(@enum, out var audioInfo))
        {
            if (audioInfo.Clip == null)
            {
                Debug.LogError($"PlaySound 音频已被销毁：{@enum}");
                return;
            }

            if (audioInfo.CanMultiplePlay)
            {
                double currentTime = Time.unscaledTimeAsDouble;
                if (audioInfo.MultiplePlayInterval > 0f &&
                    currentTime - audioInfo.LastPlayTime < audioInfo.MultiplePlayInterval)
                {
                    return;
                }

                //遍历找空闲的audioSource
                AudioSource targetAudioSource = null;
                AudioSource[] audioSources = audioPoolLayer.GetComponents<AudioSource>();
                for (int i = 0; i < audioSources.Length; i++)
                {
                    //移除的组件要等帧末才销毁，不能再次复用。
                    if (_audioPoolSources.ContainsKey(audioSources[i]) && !audioSources[i].isPlaying)
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
                _audioPoolSources[targetAudioSource] = @enum;
                targetAudioSource.Play();
                audioInfo.LastPlayTime = currentTime;
            }
            else
            {
                //获取当前只能存在唯一的音效看有没有对应的播放器
                if (!_audioSources.TryGetValue(@enum, out AudioSource audioSource) || audioSource == null)
                {
                    //创建播放器
                    audioSource = audioLayer.AddComponent<AudioSource>();
                    _audioSources[@enum] = audioSource;
                }

                audioSource.clip = audioInfo.Clip;
                audioSource.spatialBlend = 0f;
                audioSource.volume = _soundVolume;

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
        var cancelTokenSource = _musicCompleteCbCancelTokenS;
        _musicCompleteCbCancelTokenS = null;
        if (cancelTokenSource != null)
        {
            cancelTokenSource.Cancel();
            cancelTokenSource.Dispose();
        }

        if (_musicAudioSource != null)
        {
            _musicAudioSource.Stop();
        }

        _bePause = false;
    }

    /// <summary>
    /// 停止音效
    /// </summary>
    public override void StopSound()
    {
        StopSound(audioLayer);
        StopSound(audioPoolLayer);
    }

    /// <summary>
    /// 暂停音乐
    /// </summary>
    public override void PauseMusic()
    {
        if (_musicAudioSource != null && _musicAudioSource.clip != null && _musicAudioSource.isPlaying)
        {
            _bePause = true;
            _musicAudioSource.Pause();
        }
    }

    /// <summary>
    /// 恢复暂停的音乐
    /// </summary>
    /// <returns>返回是否恢复成功,如果之前未暂停或者未播放过音乐,则返回失败</returns>
    public override bool ResumeMusic()
    {
        if (!MusicBePause())
        {
            return false;
        }

        _bePause = false;
        _musicAudioSource.UnPause();
        return true;
    }

    /// <summary>
    /// 音乐是否处于暂停状态
    /// </summary>
    /// <returns>如果之前未播放过音乐或者音乐正在播放则返回false</returns>
    public override bool MusicBePause()
    {
        return _musicAudioSource != null && _musicAudioSource.clip != null &&
               !_musicAudioSource.isPlaying && _bePause;
    }

    private void StopSound(GameObject parentObject)
    {
        AudioSource[] audioSources = parentObject.GetComponents<AudioSource>();
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].Stop();
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
        UnityAction<string, string> completeCb = null
    );

    /// <summary>
    /// 播放音乐,仅一个音源,播放另外一个时,会停止之前的
    /// </summary>
    /// <param name="enum">音乐的枚举</param>
    /// <param name="completeFlag">完成时回调的标签,不需要可以为空,会原样传回</param>
    /// <param name="completeCb">完成时的回调,不需要可以为空</param>
    /// <param name="beLoop">是否需要loop,默认需要</param>
    public abstract void PlayMusic(string @enum, bool beLoop = true, string completeFlag = "",
        UnityAction<string, string> completeCb = null
    );


    /// <summary>
    /// 播放音效,会根据添加时的
    /// </summary>
    /// <param name="enum"></param>
    public abstract void PlaySound(Enum @enum);

    /// <summary>
    /// 播放音效,会根据添加时的
    /// </summary>
    /// <param name="enum"></param>
    public abstract void PlaySound(string @enum);

    /// <summary>
    /// 增加音频，可叠加音效的最短播放间隔默认为 0.1 秒
    /// </summary>
    /// <param name="enum"></param>
    /// <param name="audioClip"></param>
    /// <param name="forceReplay">这个音频如果正在播放时,如果再次播放,则强制从头开始播放(canMultiplePlay=false时生效)</param>
    /// <param name="canMultiplePlay">这个音频是否可同时播放多个,不允许时,</param>
    /// <param name="multiplePlayInterval">可叠加音效的间隔秒数，0 表示不限制；音乐不受影响。</param>
    public abstract void AddAudioClip(Enum @enum, AudioClip audioClip, bool canMultiplePlay = true,
        bool forceReplay = false, float multiplePlayInterval = 0.1f);

    /// <summary>
    /// 增加音频，可叠加音效的最短播放间隔默认为 0.1 秒
    /// </summary>
    /// <param name="enum"></param>
    /// <param name="audioClip"></param>
    /// <param name="forceReplay">这个音频如果正在播放时,如果再次播放,则强制从头开始播放(canMultiplePlay=false时生效)</param>
    /// <param name="canMultiplePlay">这个音频是否可同时播放多个,不允许时,</param>
    /// <param name="multiplePlayInterval">可叠加音效的间隔秒数，0 表示不限制；音乐不受影响。</param>
    public abstract void AddAudioClip(string @enum, AudioClip audioClip, bool canMultiplePlay = true,
        bool forceReplay = false, float multiplePlayInterval = 0.1f);

    /// <summary>
    /// 移除音频
    /// </summary>
    /// <param name="enum"></param>
    public abstract void RemoveAudioClip(Enum @enum);

    /// <summary>
    /// 移除音频
    /// </summary>
    /// <param name="enum"></param>
    public abstract void RemoveAudioClip(string @enum);

    /// <summary>
    /// 获取clip
    /// </summary>
    /// <param name="enum"></param>
    /// <returns></returns>
    public abstract AudioClip GetAudioClip(Enum @enum);

    /// <summary>
    /// 获取clip
    /// </summary>
    /// <param name="enum"></param>
    /// <returns></returns>
    public abstract AudioClip GetAudioClip(string @enum);

    /// <summary>
    /// 背景音乐是否在播放
    /// </summary>
    public abstract bool beMusicPlaying { get; }

    /// <summary>
    /// 正在或者上一个播放的音乐
    /// </summary>
    public abstract string lastMusic { get; }

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

    /// <summary>
    /// 暂停音乐
    /// </summary>
    public abstract void PauseMusic();

    /// <summary>
    /// 恢复暂停的音乐
    /// </summary>
    /// <returns>返回是否恢复成功,如果之前未暂停或者未播放过音乐,则返回失败</returns>
    public abstract bool ResumeMusic();

    /// <summary>
    /// 音乐是否处于暂停状态
    /// </summary>
    /// <returns>如果之前未播放过音乐或者音乐正在播放则返回false</returns>
    public abstract bool MusicBePause();

    protected class AudioInfo
    {
        public AudioClip Clip;
        public bool ForceReplay;
        public bool CanMultiplePlay;
        public float MultiplePlayInterval=0.1f;
        public double LastPlayTime = double.NegativeInfinity;
    }
}
