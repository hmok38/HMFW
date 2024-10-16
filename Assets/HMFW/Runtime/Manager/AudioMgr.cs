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

    public readonly Dictionary<Enum, AudioClip> AudioClips = new Dictionary<Enum, AudioClip>();

    private readonly AudioSource _musicAudioSource;
    private UnityAction<Enum, string> _musicCompleteCb;
    private Enum _lastMusicEnum;
    private string _lastMusicFlagStr;

    private bool _musicBeLoop;

    CancellationTokenSource _musicCompleteCbCancelTokenS = new CancellationTokenSource();

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


    public override float soundVolume
    {
        get => _soundVolume;
        set
        {
            var v = Mathf.Clamp(value, 0f, 1f);
            PlayerPrefs.SetFloat(SoundPrefsKey, v);
            _soundVolume = v;
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
    public override void AddAudioClip(Enum @enum, AudioClip audioClip)
    {
        AudioClips[@enum] = audioClip;
    }

    /// <summary>
    /// 移除音频
    /// </summary>
    /// <param name="enum"></param>
    /// <param name="audioClip"></param>
    public override void RemoveAudioClip(Enum @enum, AudioClip audioClip)
    {
        AudioClips.Remove(@enum);
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
        if (AudioClips.TryGetValue(@enum, out var music))
        {
            try
            {
                if (_musicCompleteCbCancelTokenS != null)
                {
                    _musicCompleteCbCancelTokenS.Cancel(); //取消之前等待的执行逻辑
                    _musicCompleteCbCancelTokenS.Dispose();
                }

                _musicCompleteCbCancelTokenS = new CancellationTokenSource();


                _musicAudioSource.clip = music;
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

    public override void PlaySound(Enum @enum)
    {
        if (AudioClips.TryGetValue(@enum, out var music))
        {
            AudioSource.PlayClipAtPoint(music, Vector3.zero, _soundVolume);
        }
        else
        {
            Debug.LogError($"PlayMusic not exist music : {@enum}");
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
    /// 播放音效
    /// </summary>
    /// <param name="enum"></param>
    public abstract void PlaySound(Enum @enum);

    /// <summary>
    /// 增加音频
    /// </summary>
    /// <param name="enum"></param>
    /// <param name="audioClip"></param>
    public abstract void AddAudioClip(Enum @enum, AudioClip audioClip);

    /// <summary>
    /// 移除音频
    /// </summary>
    /// <param name="enum"></param>
    /// <param name="audioClip"></param>
    public abstract void RemoveAudioClip(Enum @enum, AudioClip audioClip);


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
}