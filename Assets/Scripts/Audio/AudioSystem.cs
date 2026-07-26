using UnityEngine;
using System.Collections;
using Raccoin.Core;

namespace Raccoin.Audio
{
    /// <summary>
    /// 音频管理器 - 复刻原版 AudioManager
    /// iOS 使用 Unity Audio 替代 Wwise（或集成 Wwise iOS SDK）
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        private AudioSource _sfxSource;
        private AudioSource _musicSource;
        private float _masterVolume = 1.0f;
        private float _sfxVolume = 1.0f;
        private float _musicVolume = 1.0f;

        public void Initialize(GameObject audioObj)
        {
            _sfxSource = audioObj.AddComponent<AudioSource>();
            _musicSource = audioObj.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _sfxSource.playOnAwake = false;
            _musicSource.playOnAwake = false;
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
        {
            if (_sfxSource == null || clip == null) return;
            _sfxSource.PlayOneShot(clip, volumeScale * _sfxVolume * _masterVolume);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (_musicSource == null || clip == null) return;
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.volume = _musicVolume * _masterVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource?.Stop();
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
                _musicSource.volume = _musicVolume * _masterVolume;
        }

        private void ApplyVolumes()
        {
            if (_musicSource != null)
                _musicSource.volume = _musicVolume * _masterVolume;
        }

        public IEnumerator IE_SetSoundState(bool mute)
        {
            AudioListener.pause = mute;
            yield return null;
        }
    }

    /// <summary>
    /// 音乐管理器 - 复刻原版 MusicManager
    /// </summary>
    public class MusicManager : MonoSingleton<MusicManager>
    {
        [SerializeField] private AudioClip _titleBGM;
        [SerializeField] private AudioClip _gameplayBGM;
        [SerializeField] private AudioClip _bossBGM;
        [SerializeField] private float _fadeDuration = 1.0f;

        private BGMState _currentState = BGMState.None;

        public BGMState CurrentState => _currentState;

        public void PlayTitleMusic()
        {
            if (_currentState == BGMState.Title) return;
            _currentState = BGMState.Title;
            AudioManager.Instance.PlayMusic(_titleBGM);
        }

        public void PlayGameplayMusic()
        {
            if (_currentState == BGMState.Gameplay) return;
            _currentState = BGMState.Gameplay;
            AudioManager.Instance.PlayMusic(_gameplayBGM);
        }

        public void PlayBossMusic()
        {
            if (_currentState == BGMState.Boss) return;
            _currentState = BGMState.Boss;
            AudioManager.Instance.PlayMusic(_bossBGM);
        }

        public IEnumerator IE_CrossFade(AudioClip target, float duration)
        {
            // 交叉淡入淡出
            yield return new WaitForSeconds(duration);
        }
    }

    public enum BGMState
    {
        None = 0,
        Title = 1,
        Gameplay = 2,
        Boss = 3,
        GameOver = 4
    }

    /// <summary>
    /// Wwise 事件发送器 - 复刻原版 PostWwiseEvent
    /// iOS 版本使用 Unity Audio 事件替代
    /// </summary>
    public class PostWwiseEvent : MonoBehaviour
    {
        [SerializeField] private AudioName _eventName;
        [SerializeField] private AudioClip _overrideClip;
        [SerializeField] private bool _playOnEnable = true;

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                PostEvent();
            }
        }

        public void PostEvent()
        {
            if (_overrideClip != null)
            {
                AudioManager.Instance.PlaySFX(_overrideClip);
            }
        }
    }

    /// <summary>
    /// 硬币音频 - 复刻原版 CoinAudio
    /// </summary>
    public class CoinAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip _dropClip;
        [SerializeField] private AudioClip _settleClip;
        [SerializeField] private AudioClip _destroyClip;
        [SerializeField] private float _pitchVariation = 0.1f;

        public void PlayDrop()
        {
            PlayWithVariation(_dropClip);
        }

        public void PlaySettle()
        {
            PlayWithVariation(_settleClip);
        }

        public void PlayDestroy()
        {
            PlayWithVariation(_destroyClip);
        }

        private void PlayWithVariation(AudioClip clip)
        {
            if (clip == null) return;
            // 随机音高变化，避免重复感
            AudioManager.Instance.PlaySFX(clip, Random.Range(1f - _pitchVariation, 1f + _pitchVariation));
        }
    }

    public enum AudioName
    {
        None = 0,
        CoinDrop = 1,
        CoinSettle = 2,
        CoinDestroy = 3,
        PusherMove = 4,
        UIClick = 5,
        UIBack = 6,
        RoundStart = 7,
        RoundEnd = 8,
        LuckyWheelSpin = 9,
        RPG_Attack = 10,
        RPG_Defend = 11,
        RPG_Heal = 12,
        RPG_EnemyDead = 13,
        ChipSelfDestroy = 14,
        RobotPush = 15,
        GiftOpen = 16,
        PrizeCollect = 17
    }
}
