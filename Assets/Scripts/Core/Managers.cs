using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Raccoin.Core
{
    /// <summary>
    /// 资源管理器 - 复刻原版 ResManager
    /// 使用 Addressables 进行资源加载
    /// </summary>
    public class ResManager : Singleton<ResManager>
    {
        private Dictionary<string, GameObject> _prefabCache = new();
        private Dictionary<string, Sprite> _spriteCache = new();
        private Dictionary<string, Material> _materialCache = new();
        private bool _isInitialized;

        public IEnumerator IE_InitRes()
        {
            // 初始化 Addressables
            yield return IE_LoadPrefab();
            yield return IE_LoadIconSprite();
            yield return IE_LoadMaterial();
            _isInitialized = true;
        }

        private IEnumerator IE_LoadPrefab()
        {
            // 加载核心 Prefab
            yield return null;
        }

        private IEnumerator IE_LoadIconSprite()
        {
            // 加载图标 Sprite
            yield return null;
        }

        private IEnumerator IE_LoadMaterial()
        {
            // 加载材质
            yield return null;
        }

        public IEnumerator IE_LoadDLC1()
        {
            // 加载 DLC 内容
            yield return null;
        }

        public GameObject GetPrefab(string key)
        {
            return _prefabCache.TryGetValue(key, out var prefab) ? prefab : null;
        }

        public Sprite GetSprite(string key)
        {
            return _spriteCache.TryGetValue(key, out var sprite) ? sprite : null;
        }

        public Material GetMaterial(string key)
        {
            return _materialCache.TryGetValue(key, out var mat) ? mat : null;
        }

        public void ClearCache()
        {
            _prefabCache.Clear();
            _spriteCache.Clear();
            _materialCache.Clear();
        }
    }

    /// <summary>
    /// 设置管理器 - 复刻原版 SettingManager
    /// </summary>
    public class SettingManager : Singleton<SettingManager>
    {
        public float MasterVolume { get; set; } = 1.0f;
        public float SFXVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 1.0f;
        public int QualityLevel { get; set; } = 2;
        public int TargetFPS { get; set; } = 60;
        public LanguageEnum Language { get; set; } = LanguageEnum.English;

        public void ApplySettings()
        {
            Application.targetFrameRate = TargetFPS;
            QualitySettings.SetQualityLevel(QualityLevel);
        }

        public IEnumerator IE_ChangeResolution(RabbitResolution resolution)
        {
            Screen.SetResolution(resolution.Width, resolution.Height, Screen.fullScreenMode);
            yield return null;
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
            PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            PlayerPrefs.SetInt("QualityLevel", QualityLevel);
            PlayerPrefs.SetInt("TargetFPS", TargetFPS);
            PlayerPrefs.SetInt("Language", (int)Language);
            PlayerPrefs.Save();
        }

        public void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
            QualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
            TargetFPS = PlayerPrefs.GetInt("TargetFPS", 60);
            Language = (LanguageEnum)PlayerPrefs.GetInt("Language", (int)LanguageEnum.English);
        }
    }

    public struct RabbitResolution
    {
        public int Width;
        public int Height;
    }

    public enum LanguageEnum
    {
        English = 0,
        SimplifiedChinese = 1,
        TraditionalChinese = 2,
        Japanese = 3,
        Korean = 4
    }

    public enum AudioVolumeType
    {
        Master = 0,
        SFX = 1,
        Music = 2
    }

    /// <summary>
    /// 游戏时间管理器 - 复刻原版 GameTimeManager
    /// </summary>
    public class GameTimeManager : Singleton<GameTimeManager>
    {
        public float TimeScale { get; private set; } = 1.0f;
        public bool IsPaused { get; private set; }

        public void SetTimeScale(float scale)
        {
            TimeScale = Mathf.Clamp(scale, 0.1f, 10f);
            Time.timeScale = TimeScale;
        }

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = TimeScale;
        }
    }

    /// <summary>
    /// 性能管理器 - 复刻原版 PerformanceManager
    /// </summary>
    public class PerformanceManager : Singleton<PerformanceManager>
    {
        public int MaxCoinCount { get; set; } = 500;
        public bool EnableVSync { get; set; }
        public int PhysicsSubSteps { get; set; } = 4;

        public void OptimizeForMobile()
        {
            MaxCoinCount = 300;
            PhysicsSubSteps = 2;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        public void OptimizeForHighEnd()
        {
            MaxCoinCount = 800;
            PhysicsSubSteps = 8;
            Application.targetFrameRate = 120;
        }
    }
}
